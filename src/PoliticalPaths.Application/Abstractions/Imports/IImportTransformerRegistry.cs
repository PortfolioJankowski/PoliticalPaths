namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportTransformerRegistry
{
    IImportTransformer? Resolve(string pipelineKey);
}
