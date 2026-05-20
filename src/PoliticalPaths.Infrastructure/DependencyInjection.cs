using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Infrastructure.Imports;
using PoliticalPaths.Infrastructure.Persistence;

namespace PoliticalPaths.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        var serverVersion = new MySqlServerVersion(new Version(11, 4, 0));
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IFileChecksumService, FileChecksumService>();
        services.AddScoped<IRawImportRowWriter, RawImportRowWriter>();
        services.AddSingleton<IImportLogContext, SerilogImportLogContext>();

        return services;
    }
}
