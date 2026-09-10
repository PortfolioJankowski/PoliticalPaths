using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using PoliticalPaths.Infrastructure.Identity;

namespace PoliticalPaths.Dashboard.Pages.Account;

[EnableRateLimiting("login")]
public sealed class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");

        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.FindByEmailAsync(Input.Email.Trim());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowy login lub hasło.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user, Input.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Konto jest tymczasowo zablokowane. Spróbuj ponownie później.");
        else if (!result.Succeeded)
            ModelState.AddModelError(string.Empty, "Nieprawidłowy login lub hasło.");
        else
            return LocalRedirect(ReturnUrl ?? "/");

        return Page();
    }

    public sealed class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
