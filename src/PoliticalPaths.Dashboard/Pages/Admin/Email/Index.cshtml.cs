using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages.Admin.Email;

public sealed class IndexModel(EmailCampaignService campaigns) : PageModel
{
    [BindProperty] public EmailInput Input { get; set; } = new();
    public int SubscriberCount { get; private set; }
    public IReadOnlyList<EmailCampaignRow> Recent { get; private set; } = [];
    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) { await LoadAsync(ct); return Page(); }
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administratorId)) return Challenge();
        var recipientCount = await campaigns.GetSubscriberCountAsync(ct);
        if (recipientCount == 0)
        {
            ModelState.AddModelError(string.Empty, "Brak aktywnych użytkowników, którzy wyrazili zgodę na alerty.");
            await LoadAsync(ct);
            return Page();
        }
        await campaigns.CreateAsync(administratorId, Input.Subject, Input.Body, ct);
        StatusMessage = $"Kampania została dodana do kolejki dla {recipientCount} odbiorców.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        SubscriberCount = await campaigns.GetSubscriberCountAsync(ct);
        Recent = await campaigns.GetRecentAsync(ct);
    }

    public sealed class EmailInput
    {
        [Required, StringLength(200, MinimumLength = 3)] public string Subject { get; set; } = "";
        [Required, StringLength(10000, MinimumLength = 10)] public string Body { get; set; } = "";
    }
}
