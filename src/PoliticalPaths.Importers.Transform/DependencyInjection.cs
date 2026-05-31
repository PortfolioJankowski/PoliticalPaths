using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Importers.Transform;

public static class DependencyInjection
{
    public static IServiceCollection AddTransformImporters(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IImportTransformer).IsAssignableFrom(type) || type is not { IsClass: true, IsAbstract: false })
                continue;

            if (type.GetCustomAttribute<ImportTransformerAttribute>() is null)
                continue;

            services.AddScoped(type);
        }

        return services;
    }
}
