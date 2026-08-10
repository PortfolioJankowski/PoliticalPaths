namespace PoliticalPaths.Shared.Dtos.Domain;

public record SzczegolyOkreguDto(
    Guid OkregId,
    int RokWyborow,
    int Mieszkancy,
    int Uprawnieni,
    int LiczbaMandatow,
    int LiczbaList,
    int LiczbaKandydatow,
    Guid WyboryId);

