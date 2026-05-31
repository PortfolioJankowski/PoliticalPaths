namespace PoliticalPaths.Shared.Paths;

public static class RepoPaths
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "PoliticalPaths.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string SourceDataRoot(string? repoRoot = null)
        => Path.Combine(repoRoot ?? FindRepoRoot(), "source-data");

    public static string InboxDirectory(string? repoRoot = null)
        => Path.Combine(SourceDataRoot(repoRoot ?? FindRepoRoot()), "inbox");
}
