using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Imports.Inbox;

namespace PoliticalPaths.Importers.Raw;

public static class DependencyInjection
{
    public static IServiceCollection AddRawImporters(this IServiceCollection services)
    {
        services.AddSingleton<IPipelineRegistry, PipelineRegistry>();
        services.AddScoped<GenericExcelRawImporter>();
        services.AddScoped<SejmDemo2023RawImporter>();
        services.AddScoped<IRawImporterRegistry, RawImporterRegistry>();
        services.AddSingleton<ISampleDataSeeder, SampleDataSeeder>();
        return services;
    }
}
