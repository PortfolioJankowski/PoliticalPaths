using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Application.Imports.Inbox;
using PoliticalPaths.Application.Imports.Transform;

namespace PoliticalPaths.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton<IInboxScanner, InboxScanner>();
        services.AddScoped<IImportSyncService, ImportSyncService>();
        services.AddScoped<ITransformationErrorRecorder, TransformationErrorRecorder>();
        return services;
    }
}
