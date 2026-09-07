using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;
namespace PoliticalPaths.Dashboard.Pages.Imports;
public sealed class IndexModel(DashboardQueryService queries) : PageModel
{
    public List<ImportFileListItem> Files { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) => Files = await queries.GetImportsAsync(ct);
}
