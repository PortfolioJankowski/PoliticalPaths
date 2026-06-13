using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

[ImportTransformer("Sejm2023", "sejm-2023-okregi", "sejm-2023-kandydaci")]
public sealed class SejmDemo2023Transformer(
    IAppDbContext db,
    IEntityResolver entityResolver,
    ITransformationErrorRecorder errorRecorder,
    ILogger<SejmDemo2023Transformer> logger)
    : ExcelFileTransformerBase(db, errorRecorder, logger)
{
    private const string ElectionName = "Sejm Rzeczypospolitej Polskiej";
    private const string DistrictFileMarker = "okregi";
    private static readonly string[] TrueValues = ["tak", "true", "1"];
    private const string UnknownValue = "Nieznane";

    public override string PipelineKey => "Sejm2023";

    public override async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Resolve basic election context
        var slownik = await entityResolver.GetOrCreateSlownikWyborowAsync(ElectionName, ct: cancellationToken);
        if (!int.TryParse(context.ElectionYear, out var rok)) rok = 2023;
        var wybory = await entityResolver.GetOrCreateWyboryAsync(slownik.Id, new DateOnly(rok, 10, 15), cancellationToken);

        var result = await ProcessRowsAsync(file, workbook, async (excelRow, importRow, ct) =>
        {
            if (file.StoragePath.Contains(DistrictFileMarker))
            {
                await ProcessDistrictRow(excelRow, importRow, wybory.Id, rok, ct);
            }
            else
            {
                await ProcessCandidateRow(excelRow, importRow, wybory.Id, ct);
            }
        }, cancellationToken);

        await Db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task ProcessDistrictRow(RawRowDto excelRow, ImportRow importRow, Guid wyboryId, int rok, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, DistrictsHeaders.NumerOkręgu);
        var liczbaMandatow = ParseInt(excelRow, DistrictsHeaders.LiczbaMandatów) ?? 0;
        var mieszkancy = ParseInt(excelRow, DistrictsHeaders.LiczbaMieszkańców) ?? 0;
        var uprawnieni = ParseInt(excelRow, DistrictsHeaders.LiczbaWyborców) ?? 0;
        var liczbaList = ParseInt(excelRow, DistrictsHeaders.LiczbaList);
        var liczbaKandydatow = ParseInt(excelRow, DistrictsHeaders.LiczbaKandydatow);

        if (nrOkregu == null) throw new Exception("Brak numeru okręgu");

        var okreg = await entityResolver.GetOrCreateOkregAsync(nrOkregu.Value, wyboryId, ct);
        await entityResolver.UpdateOkregDetailsAsync(okreg.Id, liczbaMandatow, liczbaList, liczbaKandydatow, ct: ct);
        await entityResolver.GetOrCreateLudnoscOkregowAsync(okreg.Id, rok, mieszkancy, uprawnieni, ct);

        importRow.DomainEntityType = nameof(OkregWyborczy);
        importRow.DomainEntityId = okreg.Id.ToString();
    }

    private async Task ProcessCandidateRow(RawRowDto excelRow, ImportRow importRow, Guid wyboryId, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, CandidatesHeaders.NrOkręgu);
        var nrListy = ParseInt(excelRow, CandidatesHeaders.NrListy);
        var pozycja = ParseInt(excelRow, CandidatesHeaders.PozycjaNaLiście);
        var nazwiskoImiona = GetValue(excelRow, CandidatesHeaders.NazwiskoIImiona);
        var komitetNazwa = GetValue(excelRow, CandidatesHeaders.NazwaKomitetu);
        var partiaNazwa = GetValue(excelRow, CandidatesHeaders.PrzynależnośćDoPartii);
        var glosy = ParseInt(excelRow, CandidatesHeaders.LiczbaGłosów) ?? 0;
        var czyMandat = ParseBool(excelRow, CandidatesHeaders.CzyPrzyznanoMandat, TrueValues);

        if (nrOkregu == null || nazwiskoImiona == null || komitetNazwa == null)
            throw new Exception("Brak wymaganych danych kandydata");

        var okreg = await entityResolver.GetOrCreateOkregAsync(nrOkregu.Value, wyboryId, ct);
        var komitet = await entityResolver.GetOrCreateKomitetAsync(komitetNazwa, ct);
        
        Guid? listaId = null;
        if (nrListy != null)
        {
            var lista = await entityResolver.GetOrCreateListaAsync(okreg.Id, wyboryId, komitet.Id, nrListy.Value, ct);
            listaId = lista.Id;
        }

        var partia = await entityResolver.GetOrCreatePartiaAsync(partiaNazwa!, ct);

        var parts = nazwiskoImiona.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var nazwisko = parts.Length > 0 ? parts[0] : UnknownValue;
        var imie = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : UnknownValue;
        var polityk = await entityResolver.GetOrCreatePolitykAsync(imie, nazwisko, ct);

        var start = new StartyWyborcze
        {
            Id = Guid.NewGuid(),
            PolitykId = polityk.Id,
            ListaId = listaId,
            NumerNaLiscie = pozycja,
            KomitetId = komitet.Id,
            PartiaId = partia?.Id,
            Zawod = GetValue(excelRow, CandidatesHeaders.Zawód),
            MiejsceZamieszkania = GetValue(excelRow, CandidatesHeaders.MiejsceZamieszkania)
        };
        Db.StartyWyborcze.Add(start);

        var wynik = new WynikiWyborow
        {
            StartId = start.Id,
            LiczbaGlosow = glosy,
            CzyMandat = czyMandat
        };
        Db.WynikiWyborow.Add(wynik);

        importRow.DomainEntityType = nameof(StartyWyborcze);
        importRow.DomainEntityId = start.Id.ToString();
    }

    enum CandidatesHeaders
    {
        NrOkręgu,
        NrListy,
        PozycjaNaLiście,
        NazwiskoIImiona,
        NazwaKomitetu,
        Płeć,
        Zawód,
        MiejsceZamieszkania,
        TerytMZ,
        GminaMZ,
        PrzynależnośćDoPartii,
        Poparcie,
        LiczbaGłosów,
        ProcentGłosówOddanychNaListę,
        ProcentGłosówOddanychWOkręgu,
        CzyPrzyznanoMandat,
    }

    enum DistrictsHeaders
    {
        NumerOkręgu,
        LiczbaMandatów,
        LiczbaList,
        LiczbaKandydatow,
        LiczbaMieszkańców,
        LiczbaWyborców,
        Siedziba,
        OpisGranic
    }
}
