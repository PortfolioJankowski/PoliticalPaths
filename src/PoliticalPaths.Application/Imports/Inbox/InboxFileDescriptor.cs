namespace PoliticalPaths.Application.Imports.Inbox;

public sealed record InboxFileDescriptor(
    string FilePath,
    string LogicalName,
    string FormatVersion,
    string? ElectionYear = null);
