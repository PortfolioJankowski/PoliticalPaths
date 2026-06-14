using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public record SzczegolyOkreguDto(Guid OkregId, 
    OkregWyborczy Okreg, 
    int RokWyborow, 
    int Mieszkancy, 
    int Uprawnieni, 
    int LiczbaMandatow, 
    int LiczbaList, 
    int LiczbaKandydatow);

