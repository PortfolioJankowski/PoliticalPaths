using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Application.Imports.Inbox;

public interface IInboxScanner
{
    IReadOnlyList<InboxFileDescriptor> ScanPipeline(string pipelineDirectory, PipelineDefinition pipeline);
}
