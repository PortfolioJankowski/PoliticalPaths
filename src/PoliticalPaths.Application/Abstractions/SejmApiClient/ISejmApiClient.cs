using PoliticalPaths.Shared.Dtos.Sejm;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public interface ISejmApiClient
{
    Task<ExtendSejmMembersDto> GetMembersListAsync(int election);
}