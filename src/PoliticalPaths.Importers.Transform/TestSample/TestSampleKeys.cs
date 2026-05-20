namespace PoliticalPaths.Importers.Transform.TestSample;

internal static class TestSampleKeys
{
    public static string BuildDomainId(TestSampleRowModel row) =>
        $"sample:{row.DistrictNumber}:{row.ListNumber}:{row.LastName}:{row.FirstName}".ToLowerInvariant();
}
