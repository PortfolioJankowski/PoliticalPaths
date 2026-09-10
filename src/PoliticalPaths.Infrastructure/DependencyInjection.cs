using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Infrastructure.Imports;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Infrastructure.Sejm;
using PoliticalPaths.Infrastructure.Identity;


namespace PoliticalPaths.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataProtection()
            .SetApplicationName("PoliticalPaths");

        var connectionString = configuration.GetConnectionString("MariaDb")
            ?? throw new InvalidOperationException("Connection string 'MariaDb' is not configured.");

        var serverVersion = new MySqlServerVersion(new Version(11, 4, 0));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<IFileChecksumService, FileChecksumService>();

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "PoliticalPaths_";
        });

        services.AddScoped<ISejmApiClient, SejmApiClient>();
        
        services.AddHttpClient<ISejmApiClient, SejmApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.sejm.gov.pl/sejm/");
        });
        services.AddScoped<ISejmDataExtender, SejmDataExtender>();
        return services;
    }
}
