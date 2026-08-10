namespace PoliticalPaths.Shared.Dtos.Sejm;

public record ExtendSejmMembersDto(List<SejmMemberDto> SejmMembers, SejmTermResponse Term, string TermNo);