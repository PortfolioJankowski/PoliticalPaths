using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;
namespace PoliticalPaths.Dashboard.Pages.Politicians;
public sealed class DetailsModel(DashboardQueryService queries) : PageModel
{
    public PoliticianDetails? Politician { get; private set; }
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Politician = await queries.GetPoliticianAsync(id, ct);
        return Politician is null ? NotFound() : Page();
    }
}
