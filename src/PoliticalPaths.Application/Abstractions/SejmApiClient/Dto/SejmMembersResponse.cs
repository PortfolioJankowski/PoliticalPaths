namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public record SejmMembersResponse(
    List<SejmMemberDto> SejmMembers);