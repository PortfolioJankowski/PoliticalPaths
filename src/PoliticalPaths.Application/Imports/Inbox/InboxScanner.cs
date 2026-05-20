using System.Text.Json;
using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Application.Imports.Inbox;

public sealed class InboxScanner : IInboxScanner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IReadOnlyList<InboxFileDescriptor> ScanPipeline(string pipelineDirectory, PipelineDefinition pipeline)
    {
        if (!Directory.Exists(pipelineDirectory))
            return [];

        var allowedNames = new HashSet<string>(pipeline.LogicalNames, StringComparer.OrdinalIgnoreCase);

        return Directory.EnumerateFiles(pipelineDirectory, "*.xlsx", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => ResolveDescriptor(path, allowedNames, pipeline.PipelineKey))
            .ToList();
    }

    private static InboxFileDescriptor ResolveDescriptor(
        string filePath,
        HashSet<string> allowedLogicalNames,
        string pipelineKey)
    {
        var baseName = Path.GetFileNameWithoutExtension(filePath);
        var sidecarPath = Path.Combine(Path.GetDirectoryName(filePath)!, $"{baseName}.import.json");

        string logicalName;
        string formatVersion = "v1";
        string? electionYear = null;

        if (File.Exists(sidecarPath))
        {
            var meta = JsonSerializer.Deserialize<InboxSidecarMeta>(File.ReadAllText(sidecarPath), JsonOptions)
                ?? throw new InvalidOperationException($"Invalid sidecar JSON: {sidecarPath}");

            if (string.IsNullOrWhiteSpace(meta.LogicalName))
                throw new InvalidOperationException($"Sidecar {sidecarPath} must define logicalName.");

            logicalName = meta.LogicalName.Trim();
            formatVersion = string.IsNullOrWhiteSpace(meta.FormatVersion) ? "v1" : meta.FormatVersion.Trim();
            electionYear = meta.ElectionYear;
        }
        else
        {
            logicalName = baseName;
        }

        if (!allowedLogicalNames.Contains(logicalName))
            throw new InvalidOperationException(
                $"Logical name '{logicalName}' in '{filePath}' is not allowed for pipeline '{pipelineKey}'. " +
                $"Allowed: {string.Join(", ", allowedLogicalNames.OrderBy(x => x))}");

        return new InboxFileDescriptor(filePath, logicalName, formatVersion, electionYear);
    }

    private sealed class InboxSidecarMeta
    {
        public string? LogicalName { get; init; }
        public string? FormatVersion { get; init; }
        public string? ElectionYear { get; init; }
    }
}
