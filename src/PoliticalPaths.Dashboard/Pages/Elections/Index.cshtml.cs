using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;
namespace PoliticalPaths.Dashboard.Pages.Elections;
public sealed class IndexModel(DashboardQueryService queries) : PageModel
{
    public List<ElectionListItem> Elections { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => Elections = await queries.GetElectionsAsync(ct);
}
