using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Infrastructure.Imports;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Infrastructure.Sejm;


namespace PoliticalPaths.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MariaDb")
            ?? throw new InvalidOperationException("Connection string 'MariaDb' is not configured.");

        var serverVersion = new MySqlServerVersion(new Version(11, 4, 0));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
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
