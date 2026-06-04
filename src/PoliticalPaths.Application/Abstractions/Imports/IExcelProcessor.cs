using PoliticalPaths.Application.Imports.ExcelDto;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IExcelProcessor
{
    ExcelWorkbookModel GetWorkbook(string filePath);
}
