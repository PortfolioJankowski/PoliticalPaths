using System.Text.Json;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;

namespace PoliticalPaths.Dashboard.Services;

public sealed class ElectionSourceCatalog
{
    private readonly IReadOnlyDictionary<(DateOnly Date, string? Term), string?> _sources;

    public ElectionSourceCatalog(ImportConfiguration configuration)
    {
        _sources = configuration.Data.Values
            .SelectMany(x => x)
            .GroupBy(x => (x.ElectionDate, x.Term))
            .ToDictionary(x => x.Key, x => NormalizeUrl(x.First().RawData));
    }

    public string? GetSourceUrl(DateOnly date, string? term) =>
        _sources.GetValueOrDefault((date, term));

    public static ElectionSourceCatalog Load(IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "file-mappings.json");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "file-mappings.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("Election source catalog file-mappings.json was not found.", path);

        var configuration = JsonSerializer.Deserialize<ImportConfiguration>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Unable to deserialize file-mappings.json.");
        return new ElectionSourceCatalog(configuration);
    }

    private static string? NormalizeUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : null;
}
