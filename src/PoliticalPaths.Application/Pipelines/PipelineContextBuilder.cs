using PoliticalPaths.Application.Abstractions.Imports.Deserialization;

namespace PoliticalPaths.Application.Pipelines;

public static class PipelineContextBuilder
{
    public static IReadOnlyList<PipelineExecutionContext> Build(
        ImportConfiguration config)
    {
        var result = new List<PipelineExecutionContext>();

        foreach (var kvp in config.Data)
        {
            var pipelineKey = kvp.Key;
            
            foreach (var importConfig in kvp.Value)
            {
                var context = new PipelineExecutionContext(pipelineKey ?? "default", kvp.Value);
                result.Add(context);
            }
        }

        return result;
    }
}
