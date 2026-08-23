using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Shared.Dtos.Sejm;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

/// <summary>
/// Serwis sprawdzający kto jako kolejna osoba wszedł do Sejmu z listy,
/// z której wygasł mandat
/// </summary>
public interface IMandatSuccessionResolver
{
    Task ResolveNextMandat(Polityk polityk, ExtendSejmMembersDto extendDto, CancellationToken cancellationToken);
}