using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages;

public sealed class ChangelogModel(ChangelogService changelog) : PageModel
{
    public IReadOnlyList<ChangelogRelease> Releases { get; private set; } = [];

    public void OnGet() => Releases = changelog.GetReleases();
}
