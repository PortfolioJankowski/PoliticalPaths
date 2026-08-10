using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class PartiaExtensions
{
    public static PartiaDto FromEntity(this Partia e)
    {
        return new PartiaDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Skrot = e.Skrot,
            DataZalozenia = e.DataZalozenia,
            DataZakonczeniaDzialalnosci = e.DataZakonczeniaDzialalnosci
        };
    }
}