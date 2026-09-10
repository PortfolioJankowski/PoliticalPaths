using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using PoliticalPaths.Application;
using PoliticalPaths.Dashboard;
using PoliticalPaths.Dashboard.Services;
using PoliticalPaths.Infrastructure;
using PoliticalPaths.Infrastructure.Identity;
using PoliticalPaths.Importers.Raw;

var builder = WebApplication.CreateBuilder(args);
var rateLimiting = builder.Configuration.GetSection(RateLimitingOptions.SectionName)
    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();
builder.Services.AddOptions<RateLimitingOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("PoliticalPaths");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AuthorizeFolder("/Admin", AppPolicies.AdminOnly);
});
builder.Services.AddApplication();
builder.Services.AddRawImporters();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<DashboardQueryService>();
builder.Services.AddSingleton<ChangelogService>();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnValidatePrincipal = async context =>
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal?.Identity?.IsAuthenticated != true)
            return;

        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal);
        if (user is null || !user.IsActive)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(5));
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy(AppPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    foreach (var value in rateLimiting.KnownProxies)
    {
        if (IPAddress.TryParse(value, out var address))
            options.KnownProxies.Add(address);
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var key = userId is { Length: > 0 }
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        var permitLimit = userId is { Length: > 0 }
            ? rateLimiting.AuthenticatedPermitLimit
            : rateLimiting.AnonymousPermitLimit;

        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        $"login:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimiting.LoginPermitLimit,
            Window = TimeSpan.FromMinutes(rateLimiting.LoginWindowMinutes),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        var retryAfter = TimeSpan.FromMinutes(1);
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var leaseRetryAfter))
            retryAfter = leaseRetryAfter;

        context.HttpContext.Response.Headers.RetryAfter =
            Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");
        logger.LogWarning("Request rate limit exceeded for {Path}; retry after {RetryAfterSeconds}s.",
            context.HttpContext.Request.Path, retryAfter.TotalSeconds);

        if (context.HttpContext.Request.GetTypedHeaders().Accept?.Any(x =>
                x.MediaType.Value?.Contains("json", StringComparison.OrdinalIgnoreCase) == true) == true)
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/429",
                title = "Przekroczono limit żądań",
                status = 429,
                detail = "Spróbuj ponownie po czasie podanym w nagłówku Retry-After."
            }, cancellationToken);
            return;
        }

        context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "<!doctype html><html lang=\"pl\"><meta charset=\"utf-8\"><title>Za dużo żądań</title>" +
            "<body style=\"font-family:sans-serif;max-width:42rem;margin:4rem auto\">" +
            "<h1>Za dużo żądań</h1><p>Odczekaj chwilę i spróbuj ponownie.</p></body></html>",
            cancellationToken);
    };
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);
        var path = context.Request.Path;
        if (user?.MustChangePassword == true &&
            !path.StartsWithSegments("/Account/ChangePassword") &&
            !path.StartsWithSegments("/Account/Logout"))
        {
            context.Response.Redirect("/Account/ChangePassword");
            return;
        }
    }

    await next();
});

app.MapRazorPages();
app.Run();

public partial class Program;

public static class AppPolicies
{
    public const string AdminOnly = "AdminOnly";
}
