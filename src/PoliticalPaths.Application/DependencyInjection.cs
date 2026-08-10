using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Services;

namespace PoliticalPaths.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IEntityResolver, EntityResolver>();
        services.AddScoped<ITransformationExecutor, TransformationExecutor>();
        services.AddScoped<IImportSyncService, ImportSyncService>();
        services.AddScoped<IImportReportService, ImportReportService>();
        services.AddScoped<ITransformationErrorRecorder, TransformationErrorRecorder>();
        services.AddScoped<IPipelineRegistry, PipelineRegistry>();
        services.AddScoped<IClubMembershipService, ClubMembershipService>();
        services.AddScoped<IMandateGeneratorService, MandateGeneratorService>();
        return services;
    }
}
