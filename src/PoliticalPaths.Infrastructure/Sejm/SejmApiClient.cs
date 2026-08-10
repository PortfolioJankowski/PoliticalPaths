using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Shared.Dtos.Sejm;

namespace PoliticalPaths.Infrastructure.Sejm;

internal class SejmApiClient(HttpClient httpClient, 
    ILogger<SejmApiClient> logger) : ISejmApiClient
{
    
    Dictionary<int, string> termMapping = new Dictionary<int, string>()
    {
        { 1, "I" },
        { 2, "II" },
        { 3, "III" },
        { 4, "IV" },
        { 5, "V" },
        { 6, "VI" },
        { 7, "VII" },
        { 8, "VIII" },
        { 9, "IX" },
        { 10, "X" }
    };
    
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
        
        return new ExtendSejmMembersDto(membersInfo, termInfo, termMapping.GetValueOrDefault(election));
    }
}

