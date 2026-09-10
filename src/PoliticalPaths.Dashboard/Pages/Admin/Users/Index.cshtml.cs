using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Infrastructure.Identity;

namespace PoliticalPaths.Dashboard.Pages.Admin.Users;

public sealed class IndexModel(
    UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<UserRow> Users { get; private set; } = [];

    [BindProperty]
    public CreateUserInput NewUser { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? TemporaryPassword { get; set; }

    public async Task OnGetAsync() => await LoadUsersAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!AppRoles.All.Contains(NewUser.Role, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(NewUser.Role), "Nieprawidłowa rola.");

        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var email = NewUser.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(NewUser.Email), "Użytkownik o tym adresie już istnieje.");
            await LoadUsersAsync();
            return Page();
        }

        var password = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            MustChangePassword = true,
            LockoutEnabled = true
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            AddErrors(create);
            await LoadUsersAsync();
            return Page();
        }

        var roleResult = await userManager.AddToRoleAsync(user, NewUser.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddErrors(roleResult);
            await LoadUsersAsync();
            return Page();
        }

        TemporaryPassword = password;
        StatusMessage = $"Utworzono konto {email}. Hasło tymczasowe jest widoczne tylko teraz.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound();
        if (userManager.GetUserId(User) == user.Id.ToString())
        {
            StatusMessage = "Własne hasło zmień na stronie „Zmień hasło”.";
            return RedirectToPage();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var password = GenerateTemporaryPassword();
        var result = await userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
        {
            StatusMessage = string.Join(" ", result.Errors.Select(x => x.Description));
            return RedirectToPage();
        }

        user.MustChangePassword = true;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);
        TemporaryPassword = password;
        StatusMessage = $"Zresetowano hasło konta {user.Email}. Hasło tymczasowe jest widoczne tylko teraz.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound();

        if (userManager.GetUserId(User) == user.Id.ToString() && user.IsActive)
        {
            StatusMessage = "Nie możesz zablokować własnego konta.";
            return RedirectToPage();
        }

        if (user.IsActive && await userManager.IsInRoleAsync(user, AppRoles.Admin) &&
            await IsLastActiveAdminAsync(user.Id))
        {
            StatusMessage = "Nie można zablokować ostatniego aktywnego administratora.";
            return RedirectToPage();
        }

        user.IsActive = !user.IsActive;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await userManager.UpdateSecurityStampAsync(user);
            StatusMessage = user.IsActive ? "Konto zostało odblokowane." : "Konto zostało zablokowane.";
        }
        else
            StatusMessage = string.Join(" ", result.Errors.Select(x => x.Description));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(Guid id, string role)
    {
        if (!AppRoles.All.Contains(role, StringComparer.Ordinal))
            return BadRequest();

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound();

        var currentUserId = userManager.GetUserId(User);
        if (currentUserId == user.Id.ToString() && role != AppRoles.Admin)
        {
            StatusMessage = "Nie możesz odebrać sobie roli administratora.";
            return RedirectToPage();
        }

        if (role != AppRoles.Admin && await userManager.IsInRoleAsync(user, AppRoles.Admin) &&
            await IsLastActiveAdminAsync(user.Id))
        {
            StatusMessage = "Nie można zmienić roli ostatniego aktywnego administratora.";
            return RedirectToPage();
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(role))
        {
            var add = await userManager.AddToRoleAsync(user, role);
            if (!add.Succeeded)
            {
                StatusMessage = string.Join(" ", add.Errors.Select(x => x.Description));
                return RedirectToPage();
            }
        }

        var oldRoles = roles.Where(x => x != role).ToArray();
        if (oldRoles.Length > 0)
            await userManager.RemoveFromRolesAsync(user, oldRoles);
        await userManager.UpdateSecurityStampAsync(user);
        StatusMessage = $"Zmieniono rolę konta {user.Email} na {role}.";
        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        var users = await userManager.Users.OrderBy(x => x.Email).ToListAsync();
        var rows = new List<UserRow>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            rows.Add(new UserRow(user.Id, user.Email ?? user.UserName ?? "—", roles.FirstOrDefault() ?? "—",
                user.IsActive, user.LockoutEnd > DateTimeOffset.UtcNow, user.MustChangePassword));
        }
        Users = rows;
    }

    private async Task<bool> IsLastActiveAdminAsync(Guid excludedId)
    {
        var administrators = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
        return administrators.Count(x => x.IsActive && x.Id != excludedId) == 0;
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }

    private static string GenerateTemporaryPassword()
    {
        const string alphabet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@$%";
        var bytes = RandomNumberGenerator.GetBytes(16);
        var body = new string(bytes.Select(x => alphabet[x % alphabet.Length]).ToArray());
        return $"Aa1!{body}";
    }

    public sealed class CreateUserInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = AppRoles.User;
    }

    public sealed record UserRow(Guid Id, string Email, string Role, bool IsActive, bool IsLockedOut,
        bool MustChangePassword);
}
