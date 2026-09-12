using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using PoliticalPaths.Dashboard.Services;

namespace PoliticalPaths.Dashboard.Pages;

[EnableRateLimiting("contact")]
public sealed class ContactModel(ContactSubmissionService submissions) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [TempData] public string? Confirmation { get; set; }

    public sealed class InputModel
    {
        [Required, StringLength(50)] public string Category { get; set; } = "Sugestia";
        [Required, StringLength(150, MinimumLength = 3)] public string Subject { get; set; } = "";
        [Required, StringLength(2000, MinimumLength = 10)] public string Body { get; set; } = "";
        public string? Website { get; set; }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(Input.Website)) return RedirectToPage();
        if (!ModelState.IsValid) return Page();

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return Challenge();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "unknown";
        await submissions.SubmitAsync(userId, email, Input.Category, Input.Subject, Input.Body, ct);
        Confirmation = "Dziękujemy. Wiadomość została przyjęta i zostanie przekazana administratorom.";
        return RedirectToPage();
    }
}
