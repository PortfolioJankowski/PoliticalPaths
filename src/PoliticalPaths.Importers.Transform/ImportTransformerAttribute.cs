namespace PoliticalPaths.Importers.Transform;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ImportTransformerAttribute(string pipelineKey) : Attribute
{
    public string PipelineKey { get; } = pipelineKey;
}
