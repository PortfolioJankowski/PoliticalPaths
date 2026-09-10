namespace PoliticalPaths.Dashboard.Services;

public sealed class ChangelogService(IWebHostEnvironment environment)
{
    private readonly Lazy<IReadOnlyList<ChangelogRelease>> _releases =
        new(() => Parse(ResolvePath(environment)));

    public IReadOnlyList<ChangelogRelease> GetReleases() => _releases.Value;

    public IReadOnlyList<ChangelogItem> GetLatest(int count) =>
        _releases.Value.SelectMany(release => release.Sections.SelectMany(section =>
                section.Items.Select(item => new ChangelogItem(release.Version, section.Name, item))))
            .Take(count)
            .ToArray();

    private static string ResolvePath(IWebHostEnvironment environment)
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "CHANGELOG.md"),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "CHANGELOG.md")),
            Path.Combine(AppContext.BaseDirectory, "CHANGELOG.md")
        };
        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("CHANGELOG.md was not found.", candidates[0]);
    }

    private static IReadOnlyList<ChangelogRelease> Parse(string path)
    {
        var releases = new List<ChangelogRelease>();
        string? releaseName = null;
        string? sectionName = null;
        var sections = new List<ChangelogSection>();
        var items = new List<string>();

        void FinishSection()
        {
            if (sectionName is null)
                return;
            sections.Add(new ChangelogSection(sectionName, items.ToArray()));
            items.Clear();
        }

        void FinishRelease()
        {
            if (releaseName is null)
                return;
            FinishSection();
            sectionName = null;
            releases.Add(new ChangelogRelease(releaseName, sections.ToArray()));
            sections.Clear();
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FinishRelease();
                releaseName = line[3..].Trim();
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal) && releaseName is not null)
            {
                FinishSection();
                sectionName = line[4..].Trim();
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal) && sectionName is not null)
                items.Add(line[2..].Trim());
        }

        FinishRelease();
        return releases;
    }
}

public sealed record ChangelogRelease(string Version, IReadOnlyList<ChangelogSection> Sections);
public sealed record ChangelogSection(string Name, IReadOnlyList<string> Items);
public sealed record ChangelogItem(string Version, string Section, string Text);
