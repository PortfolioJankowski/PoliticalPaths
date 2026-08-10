using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoliticalPaths.Application;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Importers.Transform;
using PoliticalPaths.Importers.Raw;
using PoliticalPaths.Infrastructure;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Dtos.Sejm;
using PoliticalPaths.Shared.Paths;
using Serilog;
using Serilog.Formatting.Compact;
using Spectre.Console;
using Table = Spectre.Console.Table;
using TableColumn = Spectre.Console.TableColumn;

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
        "extend" => await ExtendDatabaseWithSejmApi(host),
        _ => UnknownCommand(command)
    };
}


static async Task<int> ExtendDatabaseWithSejmApi(
    IHost host)
{
    await using var scope = host.Services.CreateAsyncScope();

    var sejmApiClient = scope.ServiceProvider
        .GetRequiredService<ISejmApiClient>();

    var sejmDataExtender = scope.ServiceProvider
        .GetRequiredService<ISejmDataExtender>();

    var terms = Enumerable.Range(9, 2).ToList();
    var termData = new List<ExtendSejmMembersDto>(terms.Count);

    foreach (var t in terms)
    {
        var data = await sejmApiClient.GetMembersListAsync(t);
        termData.Add(data);
    }

    foreach (var currentTerm in termData)
    {
        await sejmDataExtender.ExtendDataAsync(
            currentTerm,
            CancellationToken.None);
    }

    Console.WriteLine("COMPLETED!");

    return 0;
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

    AnsiConsole.MarkupLine($"[blue]Repo root:[/] {repoRoot}");
    AnsiConsole.MarkupLine($"[blue]Inbox:[/]     {inbox}");
    AnsiConsole.WriteLine();

    ImportSyncResult result = null!;

    await AnsiConsole.Progress()
        .AutoClear(false)
        .HideCompleted(false)
        .Columns(new ProgressColumn[] 
        {
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn(),
        })
        .StartAsync(async ctx =>
        {
            var progressMap = new Dictionary<string, ProgressTask>();

            var progressReporter = new Progress<ImportProgressInfo>(info =>
            {
                var key = $"{info.PipelineKey}_{info.FileName}";
                if (!progressMap.TryGetValue(key, out var task))
                {
                    task = ctx.AddTask($"[grey]{info.PipelineKey}[/] {info.FileName}", autoStart: true, maxValue: info.TotalRows > 0 ? info.TotalRows : 100);
                    progressMap[key] = task;
                }

                if (info.TotalRows > 0 && task.MaxValue != info.TotalRows)
                {
                    task.MaxValue = info.TotalRows;
                }

                task.Value = info.CurrentRow;
                
                if (info.IsCompleted)
                {
                    task.StopTask();
                }
            });

            result = await syncService.SyncAllAsync(
                new ImportSyncOptions(inbox, seedIfEmpty, force),
                progressReporter);
        });

    var reportService = scope.ServiceProvider.GetRequiredService<IImportReportService>();
    await reportService.GenerateReportAsync(result!);

    AnsiConsole.WriteLine();
    var table = new Table().Border<Table>(TableBorder.Rounded);
    table.AddColumn("Pipeline");
    table.AddColumn("Batch ID");
    table.AddColumn(new TableColumn("Imported").RightAligned());
    table.AddColumn(new TableColumn("Skipped").RightAligned());
    table.AddColumn(new TableColumn("Raw Rows").RightAligned());
    table.AddColumn(new TableColumn("Transformed").RightAligned());
    table.AddColumn(new TableColumn("Failed").RightAligned());
    table.AddColumn("Status");

    foreach (var pipeline in result!.Pipelines)
    {
        var status = "[green]OK[/]";
        if (pipeline.TransformSkippedNoTransformer) status = "[yellow]RAW ONLY[/]";
        else if (pipeline.RowsFailed > 0) status = "[red]FAILED[/]";

        table.AddRow(
            pipeline.PipelineKey,
            pipeline.BatchId.ToString().Substring(0, 8) + "...",
            pipeline.FilesImported.ToString(),
            pipeline.FilesSkipped.ToString(),
            pipeline.RowsRaw.ToString(),
            pipeline.RowsTransformed.ToString(),
            pipeline.RowsFailed.ToString(),
            status
        );
    }

    AnsiConsole.Write(table);

    AnsiConsole.MarkupLine($"[bold blue]Total:[/] pipelines={result.PipelinesProcessed}, files={result.FilesImported}, rows={result.TotalRowsRaw}");
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
