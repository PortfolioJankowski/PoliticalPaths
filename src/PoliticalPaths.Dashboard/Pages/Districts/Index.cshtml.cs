using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages.Districts;

public sealed class IndexModel(DashboardQueryService queries) : PageModel
{
    public DistrictHistoryOverview Overview { get; private set; } = new([]);

    public async Task OnGetAsync(CancellationToken ct) =>
        Overview = await queries.GetDistrictHistoryAsync(ct);
}
