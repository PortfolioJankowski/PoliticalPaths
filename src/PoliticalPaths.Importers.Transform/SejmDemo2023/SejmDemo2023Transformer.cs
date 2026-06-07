using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

/// <summary>
/// Demo end-to-end: wiele arkuszy → pełny model domenowy (wybory, okręgi, listy, kandydatury, wyniki, mandaty).
/// </summary>
[ImportTransformer("sejm-demo-2023")]
public sealed class SejmDemo2023Transformer(
    IAppDbContext db,
    IEntityResolver entityResolver,
    ITransformationErrorRecorder errorRecorder,
    ILogger<SejmDemo2023Transformer> logger)
    : ExcelFileTransformerBase(db, errorRecorder, logger)
{
    public override string PipelineKey => "sejm-demo-2023";

    public override async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Resolve basic election context
        var slownik = await entityResolver.GetOrCreateSlownikWyborowAsync("Sejm Rzeczypospolitej Polskiej", ct: cancellationToken);
        if (!int.TryParse(context.ElectionYear, out var rok)) rok = 2023;
        var wybory = await entityResolver.GetOrCreateWyboryAsync(slownik.Id, new DateOnly(rok, 10, 15), cancellationToken);

        var transformed = 0;
        var failed = 0;
        var warnings = 0;

        var rows = await Db.ImportRows
            .Where(r => r.ImportFileId == file.Id)
            .ToListAsync(cancellationToken);
        var rowsMap = rows.ToDictionary(r => (r.SheetName, r.RowNumber));

        foreach (var sheet in workbook.Sheets)
        {
            foreach (var excelRow in sheet.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!rowsMap.TryGetValue((sheet.Name, excelRow.RowNumber), out var importRow)) continue;

                try
                {
                    if (file.StoragePath.Contains("okregi"))
                    {
                        await ProcessDistrictRow(excelRow, importRow, wybory.Id, rok, cancellationToken);
                    }
                    else
                    {
                        await ProcessCandidateRow(excelRow, importRow, wybory.Id, cancellationToken);
                    }

                    importRow.Status = ImportRowStatus.Transformed;
                    transformed++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error transforming row {Row} in file {File}", excelRow.RowNumber, file.LogicalName);
                    RecordError(importRow, "Transform", "TRANS_ERR", ex.Message);
                    importRow.Status = ImportRowStatus.Failed;
                    failed++;
                }
            }
        }

        await Db.SaveChangesAsync(cancellationToken);
        return new TransformFileResult(transformed, failed, warnings);
    }

    private async Task ProcessDistrictRow(RawRowDto excelRow, ImportRow importRow, Guid wyboryId, int rok, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, DistrictsHeaders.NumerOkręgu);
        var liczbaMandatow = ParseInt(excelRow, DistrictsHeaders.LiczbaMandatów) ?? 0;
        var mieszkancy = ParseInt(excelRow, DistrictsHeaders.LiczbaMieszkańców) ?? 0;
        var uprawnieni = ParseInt(excelRow, DistrictsHeaders.LiczbaWyborców) ?? 0;

        if (nrOkregu == null) throw new Exception("Brak numeru okręgu");

        var okreg = await entityResolver.GetOrCreateOkregAsync(nrOkregu.Value, wyboryId, ct);
        await entityResolver.UpdateOkregDetailsAsync(okreg.Id, liczbaMandatow, ct: ct);
        await entityResolver.GetOrCreateLudnoscOkregowAsync(okreg.Id, rok, mieszkancy, uprawnieni, ct);

        importRow.DomainEntityType = nameof(OkregWyborczy);
        importRow.DomainEntityId = okreg.Id.ToString();
    }

    private async Task ProcessCandidateRow(RawRowDto excelRow, ImportRow importRow, Guid wyboryId, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, CandidatesHeaders.NrOkręgu);
        var nrListy = ParseInt(excelRow, CandidatesHeaders.NrListy);
        var pozycja = ParseInt(excelRow, CandidatesHeaders.PozycjaNaLiście);
        var nazwiskoImiona = GetVal(excelRow, CandidatesHeaders.NazwiskoIImiona);
        var komitetNazwa = GetVal(excelRow, CandidatesHeaders.NazwaKomitetu);
        var partiaNazwa = GetVal(excelRow, CandidatesHeaders.PrzynależnośćDoPartii);
        var glosy = ParseInt(excelRow, CandidatesHeaders.LiczbaGłosów) ?? 0;
        var czyMandat = GetVal(excelRow, CandidatesHeaders.CzyPrzyznanoMandat)?.ToLower() is "tak" or "true" or "1";

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
        var nazwisko = parts.Length > 0 ? parts[0] : "Nieznane";
        var imie = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "Nieznane";
        var polityk = await entityResolver.GetOrCreatePolitykAsync(imie, nazwisko, ct);

        var start = new StartyWyborcze
        {
            Id = Guid.NewGuid(),
            PolitykId = polityk.Id,
            ListaId = listaId,
            NumerNaLiscie = pozycja,
            KomitetId = komitet.Id,
            PartiaId = partia?.Id,
            Zawod = GetVal(excelRow, CandidatesHeaders.Zawód),
            MiejsceZamieszkania = GetVal(excelRow, CandidatesHeaders.MiejsceZamieszkania)
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

    private string? GetVal(RawRowDto row, CandidatesHeaders header) => 
        row.Columns.TryGetValue(header.ToString(), out var val) ? val : null;

    private int? ParseInt(RawRowDto row, CandidatesHeaders header) =>
        int.TryParse(GetVal(row, header), out var val) ? val : null;

    private string? GetVal(RawRowDto row, DistrictsHeaders header) => 
        row.Columns.TryGetValue(header.ToString(), out var val) ? val : null;

    private int? ParseInt(RawRowDto row, DistrictsHeaders header) =>
        int.TryParse(GetVal(row, header), out var val) ? val : null;

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
        Siedziba,
        LiczbaMandatów,
        LiczbaMieszkańców,
        LiczbaWyborców
    }
}
