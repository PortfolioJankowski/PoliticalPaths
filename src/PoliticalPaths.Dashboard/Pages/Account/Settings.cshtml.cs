using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Infrastructure.Identity;

namespace PoliticalPaths.Dashboard.Pages.Account;

public sealed class SettingsModel(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty] public bool EmailNotificationsEnabled { get; set; }
    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        EmailNotificationsEnabled = user.EmailNotificationsEnabled;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        if (user.EmailNotificationsEnabled != EmailNotificationsEnabled)
        {
            user.EmailNotificationsEnabled = EmailNotificationsEnabled;
            user.EmailNotificationsChangedAtUtc = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
        }
        StatusMessage = EmailNotificationsEnabled
            ? "Włączono alerty e-mail o zmianach w Political Paths."
            : "Wyłączono alerty e-mail.";
        return RedirectToPage();
    }
}
