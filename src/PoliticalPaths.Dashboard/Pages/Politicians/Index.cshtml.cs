using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;
namespace PoliticalPaths.Dashboard.Pages.Politicians;
public sealed class IndexModel(DashboardQueryService queries) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(Name = "page", SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public List<PoliticianListItem> Politicians { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => Politicians = await queries.SearchPoliticiansAsync(Q, Math.Max(1, PageNumber), 50, ct);
}
