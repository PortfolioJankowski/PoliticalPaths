namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public record ExtendSejmMembersDto(SejmMembersResponse SejmMembers, SejmTermResponse Term);