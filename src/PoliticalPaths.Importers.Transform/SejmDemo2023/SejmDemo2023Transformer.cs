using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

/// <summary>
/// Demo end-to-end: wiele arkuszy → pełny model domenowy (wybory, okręgi, listy, kandydatury, wyniki, mandaty, kluby).
/// </summary>
[ImportTransformer("sejm-demo-2023")]
public sealed class SejmDemo2023Transformer(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger<SejmDemo2023Transformer> logger)
    : PipelineTransformerBase(db, errorRecorder, logger)
{
    public override string PipelineKey => "sejm-demo-2023";

    public override async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default)
    {
        var state = new SejmDemoImportState(db);
        await state.EnsureBootstrapAsync(cancellationToken);

        var transformed = 0;
        var failed = 0;
        var warnings = 0;

        var ordered = rows
            .OrderBy(r => SheetOrder(r.SheetName))
            .ThenBy(r => r.RowNumber)
            .ToList();

        foreach (var row in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (outcome, entityId, entityType) = await TransformSheetRowAsync(state, row, cancellationToken);
            ApplyRowStatus(row, outcome);

            switch (outcome.Kind)
            {
                case RowOutcomeKind.Success:
                    transformed++;
                    break;
                case RowOutcomeKind.SuccessWithWarnings:
                    transformed++;
                    warnings += outcome.WarningCount;
                    break;
                case RowOutcomeKind.Failed:
                    failed++;
                    break;
            }

            if (entityId is not null)
            {
                row.DomainEntityType = entityType;
                row.DomainEntityId = entityId;
            }
        }

        await state.UpsertListVoteResultsAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Sejm demo transform file {FileId}: transformed={Transformed}, failed={Failed}, warnings={Warnings}",
            file.Id,
            transformed,
            failed,
            warnings);

        return new TransformFileResult(transformed, failed, warnings);
    }

    protected override Task<RowTransformOutcome> TransformRowAsync(
        ImportRow row,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException("Sejm demo uses multi-sheet TransformFileAsync.");

    private async Task<(RowTransformOutcome Outcome, string? EntityId, string? EntityType)> TransformSheetRowAsync(
        SejmDemoImportState state,
        ImportRow row,
        CancellationToken ct)
    {
        if (!RawRowColumns.TryParse(row, out var columns, out var jsonError))
        {
            RecordError(row, "parse", "SEJM_DEMO_JSON", jsonError ?? "Invalid JSON");
            return (RowTransformOutcome.Failed(), null, null);
        }

        try
        {
            return row.SheetName.ToLowerInvariant() switch
            {
                "okregi" => await TransformDistrictAsync(state, row, columns, ct),
                "listy" => await TransformListAsync(state, row, columns, ct),
                "kandydaci" => await TransformCandidateAsync(state, row, columns, ct),
                "frekwencja" => await TransformTurnoutAsync(state, row, columns, ct),
                "kluby" => await TransformClubAsync(state, row, columns, ct),
                _ => (RowTransformOutcome.Skipped(), null, null)
            };
        }
        catch (Exception ex)
        {
            RecordError(row, "transform", "SEJM_DEMO_ERROR", ex.Message);
            return (RowTransformOutcome.Failed(), null, null);
        }
    }

    private async Task<(RowTransformOutcome, string?, string?)> TransformDistrictAsync(
        SejmDemoImportState state,
        ImportRow row,
        Dictionary<string, string?> columns,
        CancellationToken ct)
    {
        var number = RawRowColumns.GetInt(columns, "Numer", "Okreg");
        var name = RawRowColumns.Get(columns, "Nazwa");
        if (number is null || string.IsNullOrEmpty(name))
        {
            RecordError(row, "okregi", "SEJM_DEMO_REQUIRED", "Numer i Nazwa okręgu są wymagane.");
            return (RowTransformOutcome.Failed(), null, null);
        }

        var teryt = RawRowColumns.Get(columns, "TERYT") ?? $"demo-{number}";
        var id = await state.GetOrCreateDistrictAsync(
            number.Value,
            name,
            RawRowColumns.GetInt(columns, "Ludnosc", "Ludność"),
            RawRowColumns.GetInt(columns, "Uprawnieni"),
            RawRowColumns.GetInt(columns, "Mandaty"),
            teryt,
            row.Id,
            ct);

        return (RowTransformOutcome.Success(), id.ToString(), "ElectoralDistrict");
    }

    private async Task<(RowTransformOutcome, string?, string?)> TransformListAsync(
        SejmDemoImportState state,
        ImportRow row,
        Dictionary<string, string?> columns,
        CancellationToken ct)
    {
        var district = RawRowColumns.GetInt(columns, "Okreg", "Okręg");
        var listNo = RawRowColumns.GetInt(columns, "NumerListy", "Lista");
        var listName = RawRowColumns.Get(columns, "NazwaListy");
        var committee = RawRowColumns.Get(columns, "Komitet");
        var shortName = RawRowColumns.Get(columns, "SkrotKomitetu", "SkrótKomitetu");
        var party = RawRowColumns.Get(columns, "Partia");

        if (district is null || listNo is null || listName is null || committee is null || shortName is null || party is null)
        {
            RecordError(row, "listy", "SEJM_DEMO_REQUIRED", "Brak wymaganych pól listy/komitetu.");
            return (RowTransformOutcome.Failed(), null, null);
        }

        var (_, listId) = await state.GetOrCreateListAsync(
            district.Value,
            listNo.Value,
            listName,
            committee,
            shortName,
            party,
            ct);

        return (RowTransformOutcome.Success(), listId.ToString(), "ElectoralList");
    }

    private async Task<(RowTransformOutcome, string?, string?)> TransformCandidateAsync(
        SejmDemoImportState state,
        ImportRow row,
        Dictionary<string, string?> columns,
        CancellationToken ct)
    {
        var district = RawRowColumns.GetInt(columns, "Okreg", "Okręg");
        var listNo = RawRowColumns.GetInt(columns, "Lista");
        var position = RawRowColumns.GetInt(columns, "Pozycja");
        var lastName = RawRowColumns.Get(columns, "Nazwisko");
        var firstName = RawRowColumns.Get(columns, "Imie", "Imię");

        if (district is null || listNo is null || position is null || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
        {
            RecordError(row, "kandydaci", "SEJM_DEMO_REQUIRED", "Okręg, Lista, Pozycja, Nazwisko i Imie są wymagane.");
            return (RowTransformOutcome.Failed(), null, null);
        }

        if (!state.Districts.ContainsKey(district.Value))
        {
            RecordError(row, "kandydaci", "SEJM_DEMO_DISTRICT", $"Okręg {district} nie został zaimportowany (arkusz Okregi).");
            return (RowTransformOutcome.Failed(), null, null);
        }

        var listKey = $"{SejmDemoImportState.ElectionKey}:{district}:{listNo}";
        if (!state.Lists.ContainsKey(listKey))
        {
            RecordError(row, "kandydaci", "SEJM_DEMO_LIST", $"Lista {listNo} w okręgu {district} nie istnieje (arkusz Listy).");
            return (RowTransformOutcome.Failed(), null, null);
        }

        var listId = state.Lists[listKey];
        var listEntity = await db.ElectoralLists.FirstAsync(l => l.Id == listId, ct);
        var politicianId = await state.GetOrCreatePoliticianAsync(lastName, firstName, ct);
        var candidacy = await state.UpsertCandidacyAsync(
            politicianId,
            listEntity.ElectoralDistrictId,
            listId,
            listEntity.ElectoralCommitteeId,
            position.Value,
            row.Id,
            ct);

        var votes = RawRowColumns.GetInt(columns, "Glosy", "Głosy");
        var elected = RawRowColumns.GetBool(columns, "Wybrany");
        await state.UpsertVoteResultAsync(
            candidacy,
            listEntity.ElectoralDistrictId,
            votes,
            RawRowColumns.GetDecimal(columns, "Procent"),
            elected,
            row.Id,
            ct);

        var warnings = 0;
        if (votes is 0 or null)
        {
            RecordWarning(row, "kandydaci", "SEJM_DEMO_ZERO_VOTES", "Brak głosów na kandydata.");
            warnings++;
        }

        return warnings > 0
            ? (RowTransformOutcome.SuccessWithWarnings(warnings), candidacy.Id.ToString(), "Candidacy")
            : (RowTransformOutcome.Success(), candidacy.Id.ToString(), "Candidacy");
    }

    private async Task<(RowTransformOutcome, string?, string?)> TransformTurnoutAsync(
        SejmDemoImportState state,
        ImportRow row,
        Dictionary<string, string?> columns,
        CancellationToken ct)
    {
        var district = RawRowColumns.GetInt(columns, "Okreg", "Okręg");
        if (district is null)
        {
            RecordError(row, "frekwencja", "SEJM_DEMO_REQUIRED", "Okreg jest wymagany.");
            return (RowTransformOutcome.Failed(), null, null);
        }

        await state.UpsertTurnoutAsync(
            district.Value,
            RawRowColumns.GetInt(columns, "Wydane"),
            RawRowColumns.GetInt(columns, "Wazne", "Ważne"),
            RawRowColumns.GetInt(columns, "Niewazne", "Nieważne"),
            RawRowColumns.GetDecimal(columns, "Frekwencja"),
            row.Id,
            ct);

        return (RowTransformOutcome.Success(), $"{state.ElectionId}:{district}", "DistrictTurnoutResult");
    }

    private async Task<(RowTransformOutcome, string?, string?)> TransformClubAsync(
        SejmDemoImportState state,
        ImportRow row,
        Dictionary<string, string?> columns,
        CancellationToken ct)
    {
        var club = RawRowColumns.Get(columns, "Klub");
        var lastName = RawRowColumns.Get(columns, "Nazwisko");
        var firstName = RawRowColumns.Get(columns, "Imie", "Imię");
        var fromRaw = RawRowColumns.Get(columns, "Od");

        if (club is null || lastName is null || firstName is null || fromRaw is null)
        {
            RecordError(row, "kluby", "SEJM_DEMO_REQUIRED", "Klub, Nazwisko, Imie i Od są wymagane.");
            return (RowTransformOutcome.Failed(), null, null);
        }

        if (!DateOnly.TryParse(fromRaw, CultureInfo.InvariantCulture, out var validFrom))
        {
            RecordError(row, "kluby", "SEJM_DEMO_DATE", $"Nieprawidłowa data: {fromRaw}");
            return (RowTransformOutcome.Failed(), null, null);
        }

        await state.UpsertClubMembershipAsync(club, lastName, firstName, validFrom, ct);
        return (RowTransformOutcome.Success(), club, "ClubMembership");
    }

    private static int SheetOrder(string sheetName) => sheetName.ToLowerInvariant() switch
    {
        "okregi" => 0,
        "listy" => 1,
        "kandydaci" => 2,
        "frekwencja" => 3,
        "kluby" => 4,
        _ => 99
    };

    private static void ApplyRowStatus(ImportRow row, RowTransformOutcome outcome)
    {
        switch (outcome.Kind)
        {
            case RowOutcomeKind.Success:
            case RowOutcomeKind.SuccessWithWarnings:
                row.Status = ImportRowStatus.Transformed;
                row.TransformedAt = DateTime.UtcNow;
                break;
            case RowOutcomeKind.Failed:
                row.Status = ImportRowStatus.Failed;
                break;
            case RowOutcomeKind.Skipped:
                row.Status = ImportRowStatus.Skipped;
                break;
        }
    }
}
