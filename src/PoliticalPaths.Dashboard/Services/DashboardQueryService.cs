using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Infrastructure.Persistence;

namespace PoliticalPaths.Dashboard.Services;

public sealed record DashboardSummary(int Files, int Elections, int Politicians, int Mandates);
public sealed record ImportFileListItem(Guid Id, string Name, string Pipeline, ImportFileStatus Status, int Rows, int FailedRows, DateTime? CompletedAt);
public sealed record ElectionListItem(
    Guid Id,
    string Type,
    DateOnly Date,
    string? Term,
    int Candidates,
    int Lists,
    int Mandates,
    string? SourceUrl);
public sealed record PoliticianListItem(Guid Id, string Name, int Elections, int Mandates);
public sealed record PoliticianDetails(Guid Id, string Name, DateOnly? BirthDate, IReadOnlyList<ParticipationItem> Participations, IReadOnlyList<MandateEventItem> Events);
public sealed record ParticipationItem(
    Guid StartId,
    DateOnly Date,
    string Election,
    int? District,
    int? DistrictResidents,
    int? DistrictEligibleVoters,
    int? DistrictSeats,
    int? DistrictLists,
    int? DistrictCandidates,
    int? ListNumber,
    int? PositionOnList,
    string? Committee,
    string? Party,
    string? SupportingParty,
    int Votes,
    decimal? ListVoteShare,
    bool Mandate,
    IReadOnlyList<ElectoralListCandidateItem> ListCandidates);
public sealed record ElectoralListCandidateItem(
    Guid PoliticianId,
    string Name,
    int? PositionOnList,
    int Votes,
    bool Mandate,
    bool IsCurrentPolitician);
public sealed record MandateEventItem(DateOnly Date, string Type, string? Description, string? Reference);
public sealed record DistrictHistoryOverview(IReadOnlyList<DistrictElectionTypeHistory> ElectionTypes);
public sealed record DistrictElectionTypeHistory(Guid ElectionTypeId, string ElectionType, IReadOnlyList<DistrictHistoryItem> Districts);
public sealed record DistrictHistoryItem(int DistrictNumber, IReadOnlyList<DistrictHistoryPoint> History);
public sealed record DistrictHistoryPoint(
    Guid ElectionId,
    DateOnly ElectionDate,
    string? Term,
    int? Residents,
    int EligibleVoters,
    int Seats,
    int Lists,
    int Candidates,
    int? ResidentsChange,
    decimal? ResidentsChangePercent);

public sealed class DashboardQueryService(AppDbContext db, ElectionSourceCatalog electionSources)
{
    public Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default) => GetSummaryCoreAsync(ct);

    private async Task<DashboardSummary> GetSummaryCoreAsync(CancellationToken ct)
    {
        var files = await db.ImportFiles.AsNoTracking().CountAsync(ct);
        var elections = await db.Wybory.AsNoTracking().CountAsync(ct);
        var politicians = await db.Politycy.AsNoTracking().CountAsync(ct);
        var mandates = await db.Mandaty.AsNoTracking().CountAsync(ct);
        return new DashboardSummary(files, elections, politicians, mandates);
    }

    public async Task<List<ImportFileListItem>> GetImportsAsync(CancellationToken ct = default) =>
        await db.ImportFiles.AsNoTracking().OrderByDescending(x => x.RawImportCompletedAt ?? DateTime.MinValue)
            // LogicalNames is stored through a value converter; use the
            // storage path here so the whole projection remains SQL-translatable.
            .Select(x => new ImportFileListItem(x.Id, x.StoragePath, x.ImportBatch.PipelineKey, x.Status, x.TotalRows, x.FailedRows,
                x.RawImportCompletedAt ?? x.ImportBatch.CompletedAt ?? x.ImportBatch.LastSyncedAt))
            .Take(200).ToListAsync(ct);

    public async Task<List<ElectionListItem>> GetElectionsAsync(CancellationToken ct = default)
    {
        var elections = await db.Wybory.AsNoTracking()
            .OrderByDescending(x => x.DataWyborow)
            .Select(x => new { x.Id, Type = x.Rodzaj.Nazwa, x.DataWyborow, x.Kadencja })
            .ToListAsync(ct);
        if (elections.Count == 0)
            return [];

        var electionIds = elections.Select(x => x.Id).ToList();
        var starts = await db.StartyWyborcze.AsNoTracking()
            .Where(x => electionIds.Contains(x.WyboryId))
            .Select(x => new { x.WyboryId, x.ListaId, Mandate = x.Wyniki.CzyMandat })
            .ToListAsync(ct);
        var aggregates = starts.GroupBy(x => x.WyboryId).ToDictionary(
            group => group.Key,
            group => new
            {
                Candidates = group.Count(),
                Lists = group.Where(x => x.ListaId.HasValue).Select(x => x.ListaId).Distinct().Count(),
                Mandates = group.Count(x => x.Mandate)
            });

        return elections.Select(x =>
        {
            aggregates.TryGetValue(x.Id, out var aggregate);
            return new ElectionListItem(
                x.Id,
                x.Type,
                x.DataWyborow,
                x.Kadencja,
                aggregate?.Candidates ?? 0,
                aggregate?.Lists ?? 0,
                aggregate?.Mandates ?? 0,
                electionSources.GetSourceUrl(x.DataWyborow, x.Kadencja));
        }).ToList();
    }

    public async Task<DistrictHistoryOverview> GetDistrictHistoryAsync(CancellationToken ct = default)
    {
        var rows = await (
            from details in db.SzczegolyOkregow.AsNoTracking()
            join district in db.OkregWyborczy.AsNoTracking() on details.OkregId equals district.Id
            join election in db.Wybory.AsNoTracking() on details.WyboryId equals election.Id
            join kind in db.RodzajeWyborow.AsNoTracking() on district.RodzajWyborowId equals kind.Id
            where election.RodzajWyborowId == district.RodzajWyborowId
            orderby kind.Nazwa, district.NumerOkregu, election.DataWyborow
            select new
            {
                ElectionTypeId = kind.Id,
                ElectionType = kind.Nazwa,
                DistrictNumber = district.NumerOkregu,
                ElectionId = election.Id,
                ElectionDate = election.DataWyborow,
                election.Kadencja,
                details.Mieszkancy,
                details.Uprawnieni,
                details.LiczbaMandatow,
                details.LiczbaList,
                details.LiczbaKandydatow
            }).ToListAsync(ct);

        var types = rows.GroupBy(x => new { x.ElectionTypeId, x.ElectionType })
            .Select(type => new DistrictElectionTypeHistory(
                type.Key.ElectionTypeId,
                type.Key.ElectionType,
                type.GroupBy(x => x.DistrictNumber)
                    .OrderBy(x => x.Key)
                    .Select(district =>
                    {
                        int? previousKnownResidents = null;
                        var history = district.OrderBy(x => x.ElectionDate).Select(x =>
                        {
                            // Starsze importy zapisywały brak wartości jako 0; traktuj go
                            // tak samo jak NULL, aby nie tworzyć fałszywych spadków.
                            var residents = x.Mieszkancy is > 0 ? x.Mieszkancy : null;
                            int? change = residents.HasValue && previousKnownResidents.HasValue
                                ? residents.Value - previousKnownResidents.Value
                                : null;
                            decimal? percent = change.HasValue && previousKnownResidents > 0
                                ? Math.Round(100m * change.Value / previousKnownResidents.Value, 2)
                                : null;
                            if (residents.HasValue)
                                previousKnownResidents = residents.Value;

                            return new DistrictHistoryPoint(
                                x.ElectionId, x.ElectionDate, x.Kadencja, residents, x.Uprawnieni,
                                x.LiczbaMandatow, x.LiczbaList, x.LiczbaKandydatow, change, percent);
                        }).OrderByDescending(x => x.ElectionDate).ToList();

                        return new DistrictHistoryItem(district.Key, history);
                    }).ToList()))
            .OrderBy(x => x.ElectionType)
            .ToList();

        return new DistrictHistoryOverview(types);
    }

    public async Task<List<PoliticianListItem>> SearchPoliticiansAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var politicians = db.Politycy.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            politicians = politicians.Where(x => x.Imie.Contains(query) || x.Nazwisko.Contains(query));
        }

        // Pomelo translates a correlated Distinct().Count() into a derived
        // table which references the outer alias (p.Id). MariaDB rejects
        // that correlation, so load the page first and aggregate its IDs in
        // a separate, SQL-translatable query.
        var pageItems = await politicians.OrderByDescending(x => x.Mandaty.Count).ThenBy(x => x.Nazwisko).ThenBy(x => x.Imie)
            .Select(x => new { x.Id, Name = x.Imie + " " + x.Nazwisko })
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        if (pageItems.Count == 0)
            return [];

        // Keep a List rather than an array: on .NET 10 the optimized
        // Guid[].Contains path can be evaluated as ReadOnlySpan<Guid> by
        // EF's expression funcletizer and fail before SQL generation.
        var ids = pageItems.Select(x => x.Id).ToList();
        var electionCounts = (await db.StartyWyborcze.AsNoTracking()
                .Where(x => ids.Contains(x.PolitykId))
                .Select(x => new { x.PolitykId, x.WyboryId })
                .ToListAsync(ct))
            .GroupBy(x => x.PolitykId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.WyboryId).Distinct().Count());

        var mandateCounts = await db.Mandaty.AsNoTracking()
            .Where(x => ids.Contains(x.PolitykId))
            .GroupBy(x => x.PolitykId)
            .Select(x => new { PolitykId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PolitykId, x => x.Count, ct);

        return pageItems.Select(x => new PoliticianListItem(
            x.Id,
            x.Name,
            electionCounts.GetValueOrDefault(x.Id),
            mandateCounts.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<PoliticianDetails?> GetPoliticianAsync(Guid id, CancellationToken ct = default)
    {
        var politician = await db.Politycy.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Imie, x.Nazwisko, x.DataUrodzenia }).SingleOrDefaultAsync(ct);
        if (politician is null) return null;

        var rawParticipations = await (
            from start in db.StartyWyborcze.AsNoTracking()
            join election in db.Wybory.AsNoTracking() on start.WyboryId equals election.Id
            join kind in db.RodzajeWyborow.AsNoTracking() on election.RodzajWyborowId equals kind.Id
            join committee in db.KomitetyWyborcze.AsNoTracking() on start.KomitetId equals committee.Id
            join list in db.ListaWyborcza.AsNoTracking() on start.ListaId equals (Guid?)list.Id into lists
            from list in lists.DefaultIfEmpty()
            join district in db.OkregWyborczy.AsNoTracking() on list.OkregId equals district.Id into districts
            from district in districts.DefaultIfEmpty()
            join party in db.Partie.AsNoTracking() on start.PartiaId equals (Guid?)party.Id into parties
            from party in parties.DefaultIfEmpty()
            join supportingParty in db.Partie.AsNoTracking() on start.PopierajacaPartiaId equals (Guid?)supportingParty.Id into supportingParties
            from supportingParty in supportingParties.DefaultIfEmpty()
            where start.PolitykId == id
            orderby election.DataWyborow descending
            select new
            {
                StartId = start.Id,
                ElectionId = election.Id,
                Date = election.DataWyborow,
                Election = kind.Nazwa,
                District = (int?)district.NumerOkregu,
                DistrictId = (Guid?)district.Id,
                ListId = (Guid?)list.Id,
                ListNumber = (int?)list.NumerListy,
                PositionOnList = start.NumerNaLiscie,
                Committee = committee.Nazwa,
                Party = party.Nazwa,
                SupportingParty = supportingParty.Nazwa,
                Votes = start.Wyniki.LiczbaGlosow,
                Mandate = start.Wyniki.CzyMandat
            }).ToListAsync(ct);

        var listIds = rawParticipations.Where(x => x.ListId.HasValue).Select(x => x.ListId!.Value).Distinct().ToList();
        var districtIds = rawParticipations.Where(x => x.DistrictId.HasValue)
            .Select(x => x.DistrictId!.Value).Distinct().ToList();
        var electionIds = rawParticipations.Select(x => x.ElectionId).Distinct().ToList();
        var districtDetails = districtIds.Count == 0
            ? []
            : await db.SzczegolyOkregow.AsNoTracking()
                .Where(x => districtIds.Contains(x.OkregId) && electionIds.Contains(x.WyboryId))
                .Select(x => new
                {
                    x.OkregId,
                    x.WyboryId,
                    x.Mieszkancy,
                    x.Uprawnieni,
                    x.LiczbaMandatow,
                    x.LiczbaList,
                    x.LiczbaKandydatow
                }).ToListAsync(ct);

        var rawListCandidates = listIds.Count == 0
            ? []
            : await db.StartyWyborcze.AsNoTracking()
                .Where(x => x.ListaId.HasValue && listIds.Contains(x.ListaId.Value))
                .Select(x => new
                {
                    ListId = x.ListaId!.Value,
                    x.PolitykId,
                    Name = x.Polityk.Imie + " " + x.Polityk.Nazwisko,
                    x.NumerNaLiscie,
                    Votes = x.Wyniki.LiczbaGlosow,
                    Mandate = x.Wyniki.CzyMandat
                })
                .OrderBy(x => x.ListId)
                .ThenBy(x => x.NumerNaLiscie)
                .ToListAsync(ct);

        var candidatesByList = rawListCandidates
            .GroupBy(x => x.ListId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ElectoralListCandidateItem>)group
                    .Select(x => new ElectoralListCandidateItem(
                        x.PolitykId, x.Name, x.NumerNaLiscie, x.Votes, x.Mandate, x.PolitykId == id))
                    .ToList());

        var participations = rawParticipations.Select(x =>
        {
            var details = districtDetails.FirstOrDefault(detail =>
                detail.OkregId == x.DistrictId && detail.WyboryId == x.ElectionId);
            var candidates = x.ListId is { } listId && candidatesByList.TryGetValue(listId, out var listCandidates)
                ? listCandidates
                : [];
            var total = candidates.Sum(candidate => candidate.Votes);

            return new ParticipationItem(
                x.StartId,
                x.Date,
                x.Election,
                x.District,
                details?.Mieszkancy,
                details?.Uprawnieni,
                details?.LiczbaMandatow,
                details?.LiczbaList,
                details?.LiczbaKandydatow,
                x.ListNumber,
                x.PositionOnList,
                x.Committee,
                x.Party,
                x.SupportingParty,
                x.Votes,
                total > 0 ? Math.Round(100m * x.Votes / total, 2) : null,
                x.Mandate,
                candidates);
        }).ToList();
        var events = await db.ZdarzeniaMandatowe.AsNoTracking().Where(x => x.PolitykId == id).OrderByDescending(x => x.DataZdarzenia)
            .Select(x => new MandateEventItem(x.DataZdarzenia, x.Typ.ToString(), x.Opis, x.DokumentReferencyjny)).ToListAsync(ct);
        return new PoliticianDetails(politician.Id, $"{politician.Imie} {politician.Nazwisko}", politician.DataUrodzenia, participations, events);
    }
}
