using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public interface ISejmApiClient
{
    Task<ExtendSejmMembersDto> GetMembersListAsync(int election);
}

public class SejmApiClient(HttpClient httpClient, 
    ILogger<SejmApiClient> logger) : ISejmApiClient
{
    public async Task<ExtendSejmMembersDto> GetMembersListAsync(int election)
    {
        var termResponse = await httpClient.GetAsync($"term{election}");
        termResponse.EnsureSuccessStatusCode();
        var termInfo = await termResponse.Content.ReadFromJsonAsync<SejmTermResponse>();

        logger.LogDebug($"[SejmApiClient]: Successfully fetched data for {election} term");
        var membersResponse = await httpClient.GetAsync($"term{election}/MP");
        membersResponse.EnsureSuccessStatusCode();
        logger.LogDebug($"[SejmApiClient]: Successfully fetched members data for {election} term");
        var membersInfo = await membersResponse.Content.ReadFromJsonAsync<List<SejmMemberDto>>();

        ArgumentNullException.ThrowIfNull(membersInfo);
        ArgumentNullException.ThrowIfNull(termInfo);
        
        return new ExtendSejmMembersDto(new SejmMembersResponse( membersInfo), termInfo);
    }
}

