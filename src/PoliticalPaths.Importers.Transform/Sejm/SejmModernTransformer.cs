using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Application.Services;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared;
using PoliticalPaths.Shared.Dtos.Domain;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

[ImportTransformer("Sejm")]
public sealed class SejmModernTransformer(
    IAppDbContext db,
    IEntityResolver entityResolver,
    ITransformationErrorRecorder errorRecorder,
    ILogger<SejmModernTransformer> logger,
    IClubMembershipService clubService)
    : ExcelFileTransformerBase(db, errorRecorder, logger)
{
    public override string PipelineKey => "Sejm";

    public override async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        string pipelineKey,
        ImportSourceDefinition source,
        IProgress<TransformationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var electionName = $"{source.Assembly}";

        var rodzajWyborow = await entityResolver.GetOrCreateSlownikWyborowAsync(electionName, ct: cancellationToken);
        
        if (!int.TryParse(source.ElectionDate.Year.ToString(), out var rok))
        {
            throw new Exception("Nie można odczytać roku wyborów z kontekstu");
        }

        var wybory = await entityResolver.GetOrCreateWyboryAsync(new WyboryDto()
        {
            RodzajWyborowId = rodzajWyborow.Id,
            CzyPrzedterminowe = source.IsSupplementary,
            DataOgloszenia = source.AnnouncementDate,
            DataWyborow = source.ElectionDate,
            Ordynacja = OrdynacjaWyborcza.Proporcjonalna,
            Tura = (TuraWyborow)int.Parse(source.Round),
            Kadencja = source.Term
        });

        var result = await ProcessRowsAsync(file, workbook, async (excelRow, importRow, ct) =>
        {
            if (file.StoragePath.Contains(TransformationConsts.DISTRICT_FILE_MARKER))
            {
                await ProcessDistrictRow(excelRow, importRow, wybory, rodzajWyborow.Id,  rok, ct);
            }
            else
            {
                await ProcessCandidateRow(excelRow, importRow, wybory, rodzajWyborow.Id, ct);
            }
        }, progress, cancellationToken);

        await Db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task ProcessDistrictRow(RawRowDto excelRow, ImportRow importRow, Wybory wybory, Guid rodzajWyborowId, int rok, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, DistrictsHeaders.NumerOkręgu);
        var liczbaMandatow = ParseInt(excelRow, DistrictsHeaders.LiczbaMandatów) ?? 0;
        var mieszkancy = ParseInt(excelRow, DistrictsHeaders.LiczbaMieszkańców) ?? 0;
        var uprawnieni = ParseInt(excelRow, DistrictsHeaders.LiczbaWyborców) ?? 0;
        var liczbaList = ParseInt(excelRow, DistrictsHeaders.LiczbaList);
        var liczbaKandydatow = ParseInt(excelRow, DistrictsHeaders.LiczbaKandydatow);

        if (nrOkregu == null) throw new Exception("Brak numeru okręgu");

        var okreg = await entityResolver.GetOrCreateOkregAsync(nrOkregu.Value, rodzajWyborowId, ct);
        
        var szczegolyOkregu = new SzczegolyOkreguDto(
            OkregId: okreg.Id,
            RokWyborow: rok,
            Mieszkancy: mieszkancy,
            Uprawnieni: uprawnieni,
            LiczbaMandatow: liczbaMandatow,
            LiczbaList: liczbaList ?? 0,
            LiczbaKandydatow: liczbaKandydatow ?? 0,
            WyboryId: wybory.Id
        );

        await entityResolver.GetOrCreateSzczegolyOkregu(szczegolyOkregu);

        importRow.DomainEntityType = nameof(OkregWyborczy);
        importRow.DomainEntityId = okreg.Id.ToString();
    }

    private async Task ProcessCandidateRow(RawRowDto excelRow, ImportRow importRow, Wybory wybory, Guid rodzajWyborowId, CancellationToken ct)
    {
        var nrOkregu = ParseInt(excelRow, CandidatesHeaders.NrOkręgu);
        var nrListy = ParseInt(excelRow, CandidatesHeaders.NrListy);
        var pozycja = ParseInt(excelRow, CandidatesHeaders.PozycjaNaLiście);
        var nazwiskoImiona = GetValue(excelRow, CandidatesHeaders.NazwiskoIImiona);
        var komitetNazwa = GetValue(excelRow, CandidatesHeaders.NazwaKomitetu);
        var partiaNazwa = GetValue(excelRow, CandidatesHeaders.PrzynależnośćDoPartii);

        if (partiaNazwa!.Contains("członek partii politycznej: "))
        {
            partiaNazwa = partiaNazwa!.Replace("członek partii politycznej: ", "", StringComparison.OrdinalIgnoreCase).Trim();
        }
        else if (partiaNazwa!.Contains("członek partii "))
        {
            partiaNazwa = partiaNazwa!.Replace("członek partii ", "", StringComparison.OrdinalIgnoreCase).Trim();
        }
        
        var popierajacaPartiaNazwa = GetValue(excelRow, CandidatesHeaders.Poparcie);
        
        if (!string.IsNullOrWhiteSpace(popierajacaPartiaNazwa))
        {
            if (popierajacaPartiaNazwa.StartsWith("popiera"))
            {
                popierajacaPartiaNazwa = popierajacaPartiaNazwa!.Replace("popierana przez partię polityczną: ", "", StringComparison.OrdinalIgnoreCase).Trim();
                popierajacaPartiaNazwa = popierajacaPartiaNazwa!.Replace("popierany przez partię polityczną: ", "", StringComparison.OrdinalIgnoreCase).Trim();
            }
        }

        var glosy = ParseInt(excelRow, CandidatesHeaders.LiczbaGłosów) ?? 0;
        var czyMandat = ParseBool(excelRow, CandidatesHeaders.CzyPrzyznanoMandat, TransformationConsts.TRUE_VALUES);

        if (nrOkregu == null || nazwiskoImiona == null || komitetNazwa == null)
            throw new Exception("Brak wymaganych danych kandydata");

        var okreg = await entityResolver.GetOrCreateOkregAsync(nrOkregu.Value, rodzajWyborowId, ct);
        var komitet = await entityResolver.GetOrCreateKomitetAsync(komitetNazwa, ct);
        
        Guid? listaId = null;
        if (nrListy != null)
        {
            var lista = await entityResolver.GetOrCreateListaAsync(okreg.Id, wybory.Id, komitet.Id, nrListy.Value, ct);
            listaId = lista.Id;
        }

        var imieNazwiskoDto = ExtractNamesAndSurnameService.Extract(nazwiskoImiona, NameExtractingOptions.GetDefault());
        
        var polityk = await entityResolver.GetOrCreatePolitykAsync(imieNazwiskoDto, ct);

        var partia = await entityResolver.GetOrCreatePartiaAsync(partiaNazwa!, ct);
        if (partia != null)
        {
            await clubService.UpdateMembershipAsync(polityk.Id, partia.Id, wybory.Id);
        }

        var popierajacaPartia = await entityResolver.GetOrCreatePartiaAsync(popierajacaPartiaNazwa!, ct);

        var wyniki = entityResolver.CreateWynikiAsync(glosy, czyMandat);

        var start = new StartWyborczy
        {
            Id = Guid.NewGuid(),
            PolitykId = polityk.Id,
            ListaId = listaId,
            NumerNaLiscie = pozycja,
            KomitetId = komitet.Id,
            PartiaId = partia?.Id,
            Zawod = GetValue(excelRow, CandidatesHeaders.Zawód),
            MiejsceZamieszkania = GetValue(excelRow, CandidatesHeaders.MiejsceZamieszkania),
            WynikiId = wyniki.Id,
            WyboryId = wybory.Id
        };

        if (popierajacaPartia != null)
        {
            start.PopierajacaPartiaId = popierajacaPartia.Id;
        }

        Db.StartyWyborcze.Add(start);

        importRow.DomainEntityType = nameof(StartWyborczy);
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
