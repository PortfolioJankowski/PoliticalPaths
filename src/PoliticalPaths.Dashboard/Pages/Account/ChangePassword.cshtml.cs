using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PoliticalPaths.Infrastructure.Identity;

namespace PoliticalPaths.Dashboard.Pages.Account;

public sealed class ChangePasswordModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public PasswordInput Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var result = await userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        user.MustChangePassword = false;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var error in update.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        SuccessMessage = "Hasło zostało zmienione.";
        return RedirectToPage("/Index");
    }

    public sealed class PasswordInput
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(12)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
