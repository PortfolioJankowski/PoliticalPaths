namespace PoliticalPaths.Importers.Transform;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ImportTransformerAttribute(string pipelineKey, params string[] logicalNames) : Attribute
{
    public string PipelineKey { get; } = pipelineKey;
    public string[] LogicalNames { get; } = logicalNames;
}
