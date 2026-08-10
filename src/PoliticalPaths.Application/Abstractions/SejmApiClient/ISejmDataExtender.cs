using PoliticalPaths.Shared.Dtos.Sejm;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

/// <summary>
/// Extends members info based on SejmAPI response.
/// </summary>
///
public interface ISejmDataExtender
{
    Task ExtendDataAsync(ExtendSejmMembersDto extendDto, CancellationToken cancellationToken);
}