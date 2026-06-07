using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoliticalPaths.Application;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Importers.Transform;
using PoliticalPaths.Importers.Raw;
using PoliticalPaths.Infrastructure;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Paths;
using Serilog;
using Serilog.Formatting.Compact;

var host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .UseSerilog((context, _, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: context.Configuration["Import:LogsPath"] ?? "logs/imports/app-.log",
                rollingInterval: RollingInterval.Day);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddRawImporters();
        services.AddTransformImporters();
    })
    .Build();

try
{
    return await RunAsync(host, args);
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task<int> RunAsync(IHost host, string[] args)
{
    var command = args.Length == 0 ? "sync" : args[0].ToLowerInvariant();

    return command switch
    {
        "help" or "--help" or "-h" => PrintHelp(),
        "sync" or "dev" => await RunSyncAsync(host, args),
        "db" => await MigrateDatabaseAsync(host, args),
        _ => UnknownCommand(command)
    };
}

static async Task<int> RunSyncAsync(IHost host, string[] args)
{
    var configuration = host.Services.GetRequiredService<IConfiguration>();
    await using var scope = host.Services.CreateAsyncScope();
    var syncService = scope.ServiceProvider.GetRequiredService<IImportSyncService>();

    var repoRoot = RepoPaths.FindRepoRoot();
    var inbox = configuration["Import:InboxPath"] is { Length: > 0 } relative
        ? Path.GetFullPath(Path.Combine(repoRoot, relative))
        : RepoPaths.InboxDirectory(repoRoot);

    var seedIfEmpty = !args.Contains("--no-seed");
    var force = args.Contains("--force");

    Console.WriteLine($"Repo root: {repoRoot}");
    Console.WriteLine($"Inbox:     {inbox}");
    Console.WriteLine();

    var result = await syncService.SyncAllAsync(
        new ImportSyncOptions(inbox, seedIfEmpty, force));

    var reportService = scope.ServiceProvider.GetRequiredService<IImportReportService>();
    await reportService.GenerateReportAsync(result);

    foreach (var pipeline in result.Pipelines)
    {
        Console.WriteLine($"Pipeline [{pipeline.PipelineKey}] batch={pipeline.BatchId}");
        Console.WriteLine($"  imported={pipeline.FilesImported}, skipped={pipeline.FilesSkipped}, rawRows={pipeline.RowsRaw}, transformed={pipeline.RowsTransformed}, failed={pipeline.RowsFailed}");
        if (pipeline.TransformSkippedNoTransformer)
            Console.WriteLine("  transform: brak transformera (tylko RAW)");
        else if (pipeline.RowsFailed > 0)
            Console.WriteLine("  transform: część wierszy FAILED — patrz TransformationErrors + logi");
        Console.WriteLine();
    }

    Console.WriteLine($"Total: pipelines={result.PipelinesProcessed}, files imported={result.FilesImported}, skipped={result.FilesSkipped}, raw rows={result.TotalRowsRaw}");
    return 0;
}

static async Task<int> MigrateDatabaseAsync(IHost host, string[] args)
{
    if (args is not ["db", "migrate"])
    {
        Console.Error.WriteLine("Usage: db migrate");
        return 1;
    }

    await using var scope = host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    Console.WriteLine("Database migrated.");
    return 0;
}

static int PrintHelp()
{
    Console.WriteLine("""
        PoliticalPaths ImportWorker

        Domyślnie (F5):  sync  (alias: dev)

        Dla każdego pipeline (transformera):
          - GetOrCreate ImportBatch po PipelineKey
          - skan source-data/inbox/{pipeline-key}/*.xlsx
          - SHA już w batch → skip
          - nowy plik → RAW + Transform (jeśli transformer zarejestrowany)

        Komendy:
          sync | dev [--no-seed] [--force]
          db migrate
          help

        Inbox: source-data/inbox/{pipeline-key}/
        """);
    return 0;
}

static int UnknownCommand(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintHelp();
    return 1;
}
