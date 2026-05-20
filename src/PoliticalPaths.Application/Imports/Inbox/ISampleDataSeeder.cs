namespace PoliticalPaths.Application.Imports.Inbox;

public interface ISampleDataSeeder
{
    /// <summary>
    /// Tworzy przykładowy plik w folderze pipeline, jeśli brak .xlsx. Zwraca ścieżkę lub null.
    /// </summary>
    string? EnsureSampleInPipelineFolder(string pipelineDirectory, string pipelineKey);
}
