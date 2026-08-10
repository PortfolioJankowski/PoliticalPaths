using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class PolitykExtensions
{
    public static PolitykDto FromEntity(this Polityk e)
    {
        return new PolitykDto
        {
            Id = e.Id,
            Imie = e.Imie,
            Nazwisko = e.Nazwisko,
            DataUrodzenia = e.DataUrodzenia,
            MiejsceUrodzenia = e.MiejsceUrodzenia,
            Email = e.Email,
            InformacjeDodatkowe = e.InformacjeDodatkowe
        };
    }
}