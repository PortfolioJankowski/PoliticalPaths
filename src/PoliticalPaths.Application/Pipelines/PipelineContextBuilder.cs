using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Application.Pipelines;

public static class PipelineContextBuilder
{
    public static IReadOnlyList<PipelineExecutionContext> Build(
        ImportConfiguration config)
    {
        var result = new List<PipelineExecutionContext>();

        foreach (var category in config.Data) 
        {
            foreach (var year in category.Value) 
            {
                var groupedByPipeline = year.Value
                    .GroupBy(x => x.Pipeline);

                foreach (var pipelineGroup in groupedByPipeline)
                {
                    result.Add(new PipelineExecutionContext(
                        ElectionType: category.Key,
                        ElectionYear: year.Key,
                        PipelineKey: pipelineGroup.Key,
                        Sources: pipelineGroup.ToList()));
                }
            }
        }

        return result;
    }
}
