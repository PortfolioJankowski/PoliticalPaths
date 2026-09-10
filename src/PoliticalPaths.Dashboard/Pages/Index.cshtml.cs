using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages;

public class IndexModel(DashboardQueryService queries, ChangelogService changelog) : PageModel
{
    public DashboardSummary Summary { get; private set; } = new(0, 0, 0, 0);
    public IReadOnlyList<ChangelogItem> LatestChanges { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Summary = await queries.GetSummaryAsync(ct);
        LatestChanges = changelog.GetLatest(3);
    }
}
