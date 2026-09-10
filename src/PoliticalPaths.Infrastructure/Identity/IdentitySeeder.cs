using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PoliticalPaths.Infrastructure.Identity;

public sealed class IdentitySeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<SeedAdminOptions> options,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Identity administrator seed is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
            throw new InvalidOperationException(
                "Identity seed is enabled, but Identity:SeedAdmin:Email or Password is missing.");

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                EnsureSucceeded(roleResult, $"create role '{role}'");
            }
        }

        var normalizedEmail = settings.Email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true,
                IsActive = true,
                MustChangePassword = false,
                LockoutEnabled = true
            };

            EnsureSucceeded(await userManager.CreateAsync(user, settings.Password), "create seed administrator");
            logger.LogInformation("Created seed administrator {Email}.", normalizedEmail);
        }

        var changed = false;
        if (!user.IsActive)
        {
            user.IsActive = true;
            changed = true;
        }

        if (changed)
            EnsureSucceeded(await userManager.UpdateAsync(user), "reactivate seed administrator");

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, AppRoles.Admin), "assign Admin role");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to {operation}: {errors}");
    }
}
