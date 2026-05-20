using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform.TestSample;

/// <summary>
/// Wzorcowy transformer — szablon pod kolejne pipeline'y (Sejm, Senat, …).
/// Nie zapisuje jeszcze encji domenowych; ustawia DomainEntityType/Id jako „suchy” wynik.
/// </summary>
[ImportTransformer("test-sample")]
public sealed class TestSampleTransformer(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger<TestSampleTransformer> logger)
    : PipelineTransformerBase(db, errorRecorder, logger)
{
    public override string PipelineKey => "test-sample";

    protected override Task<RowTransformOutcome> TransformRowAsync(
        ImportRow row,
        CancellationToken cancellationToken)
    {
        var parse = TestSampleRowParser.Parse(row);
        if (!parse.Success)
        {
            foreach (var err in parse.Errors)
                RecordError(row, "parse", err.ErrorCode, err.Message, err.Field, err.RawValue);

            return Task.FromResult(RowTransformOutcome.Failed());
        }

        var data = parse.Model!;
        var warnings = 0;

        if (data.Votes == 0)
        {
            RecordWarning(
                row,
                "validate",
                "TEST_SAMPLE_ZERO_VOTES",
                "Kandydat ma 0 głosów — sprawdź dane źródłowe.",
                "Głosy",
                "0");
            warnings++;
        }

        row.DomainEntityType = "Sample.CandidateRecord";
        row.DomainEntityId = TestSampleKeys.BuildDomainId(data);

        return Task.FromResult(
            warnings > 0
                ? RowTransformOutcome.SuccessWithWarnings(warnings)
                : RowTransformOutcome.Success());
    }
}
