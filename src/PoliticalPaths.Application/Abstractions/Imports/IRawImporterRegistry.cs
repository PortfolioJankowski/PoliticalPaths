namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IRawImporterRegistry
{
    IRawExcelImporter Resolve(string logicalName);
}
