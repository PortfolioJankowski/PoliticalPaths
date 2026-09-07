using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages;

public class IndexModel(DashboardQueryService queries) : PageModel
{
    public DashboardSummary Summary { get; private set; } = new(0, 0, 0, 0);
    public async Task OnGetAsync(CancellationToken ct) => Summary = await queries.GetSummaryAsync(ct);
}
