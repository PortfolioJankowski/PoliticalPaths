using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Importers.Raw.Excel;

namespace PoliticalPaths.Importers.Raw;

public static class DependencyInjection
{
    public static IServiceCollection AddRawImporters(this IServiceCollection services)
    {
        services.AddScoped<IExcelProcessor, ExcelProcessor>();
        return services;
    }
}
