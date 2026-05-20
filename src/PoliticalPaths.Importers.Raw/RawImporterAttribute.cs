using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Raw;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RawImporterAttribute : Attribute
{
    public string PipelineKey { get; }
    public string[] LogicalNames { get; }
    public DataSourceType DataSourceType { get; init; } = DataSourceType.GenericExcel;

    /// <param name="pipelineKey">Stabilny klucz batcha (1 pipeline = 1 batch).</param>
    /// <param name="logicalNames">Nazwy plików / logical name z sidecar.</param>
    public RawImporterAttribute(string pipelineKey, params string[] logicalNames)
    {
        PipelineKey = pipelineKey;
        LogicalNames = logicalNames;
    }
}
