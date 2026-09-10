using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoliticalPaths.Infrastructure.Identity;
using PoliticalPaths.Infrastructure.Persistence;
using Xunit;

namespace PoliticalPaths.Tests;

public sealed class DashboardSecurityTests
{
    [Fact]
    public async Task Anonymous_dashboard_redirects_to_login()
    {
        await using var factory = new DashboardFactory(authenticated: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Authenticated_user_can_view_changelog()
    {
        await using var factory = new DashboardFactory(authenticated: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/Changelog");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, html);
        Assert.Contains("Najważniejsze zmiany", html);
    }

    [Fact]
    public async Task Login_policy_returns_429_after_limit()
    {
        await using var factory = new DashboardFactory(authenticated: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage? response = null;
        for (var i = 0; i < 21; i++)
            response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.TooManyRequests, response!.StatusCode);
        Assert.True(response.Headers.Contains("Retry-After"));
    }

    private sealed class DashboardFactory(bool authenticated) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                if (authenticated)
                {
                    services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                            options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            TestAuthHandler.TestScheme, _ => { });
                }
            });
        }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string TestScheme = "Tests";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "test@example.local"),
                new Claim(ClaimTypes.Role, AppRoles.User)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, TestScheme));
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(principal, TestScheme)));
        }
    }
}
