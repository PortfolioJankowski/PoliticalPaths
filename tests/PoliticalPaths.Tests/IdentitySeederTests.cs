using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PoliticalPaths.Infrastructure.Identity;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Infrastructure;
using Xunit;

namespace PoliticalPaths.Tests;

public sealed class IdentitySeederTests
{
    [Fact]
    public void Import_worker_registration_can_resolve_user_manager()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MariaDb"] =
                    "Server=localhost;Port=3306;Database=test;Uid=test;Pwd=test;",
                ["ConnectionStrings:Redis"] = "localhost:6379"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var manager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        Assert.NotNull(manager);
    }

    [Fact]
    public async Task Seed_creates_one_admin_and_is_idempotent()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var options = Options.Create(new SeedAdminOptions
        {
            Email = "admin@example.local",
            Password = "VeryStrong1!Password"
        });
        var seeder = new IdentitySeeder(users, roles, options, NullLogger<IdentitySeeder>.Instance);

        await seeder.SeedAsync();
        var user = await users.FindByEmailAsync("admin@example.local");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, AppRoles.Admin));
        var originalHash = user.PasswordHash;

        await seeder.SeedAsync();

        Assert.Single(await users.Users.ToListAsync());
        Assert.Equal(originalHash, (await users.FindByEmailAsync("admin@example.local"))!.PasswordHash);
    }

    [Fact]
    public async Task Enabled_seed_without_credentials_fails()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var seeder = new IdentitySeeder(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
            Options.Create(new SeedAdminOptions { Enabled = true }),
            NullLogger<IdentitySeeder>.Instance);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());
        Assert.Contains("missing", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        return services.BuildServiceProvider();
    }
}
