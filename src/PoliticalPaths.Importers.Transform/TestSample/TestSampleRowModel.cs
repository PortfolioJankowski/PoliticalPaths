namespace PoliticalPaths.Importers.Transform.TestSample;

/// <summary>
/// Znormalizowany wiersz z arkusza „Kandydaci” (pipeline test-sample).
/// Docelowo zastąpisz to encjami domenowymi (Politician, Candidacy, …).
/// </summary>
public sealed record TestSampleRowModel(
    string LastName,
    string FirstName,
    int DistrictNumber,
    int ListNumber,
    int Votes);
