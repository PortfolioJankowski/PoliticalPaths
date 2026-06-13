using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Shared.Hashing;

namespace PoliticalPaths.Importers.Transform;

/// <summary>
/// Wzorzec transformera pliku Excel: odczyt workbooka, pętla po arkuszach/wierszach, statusy, zapis błędów.
/// </summary>
public abstract class ExcelFileTransformerBase(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger logger) : IImportTransformer
{
    protected readonly IAppDbContext Db = db;
    protected readonly ITransformationErrorRecorder ErrorRecorder = errorRecorder;
    protected readonly ILogger Logger = logger;

    public abstract string PipelineKey { get; }

    public abstract Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default);

    protected async Task<TransformFileResult> ProcessRowsAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        Func<RawRowDto, ImportRow, CancellationToken, Task> rowProcessor,
        CancellationToken cancellationToken)
    {
        var transformed = 0;
        var failed = 0;
        var warnings = 0;

        var rows = await Db.ImportRows
            .Where(r => r.ImportFileId == file.Id)
            .ToListAsync(cancellationToken);
        var rowsMap = rows.ToDictionary(r => (r.SheetName, r.RowNumber));

        foreach (var sheet in workbook.Sheets)
        {
            foreach (var excelRow in sheet.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (!rowsMap.TryGetValue((sheet.Name, excelRow.RowNumber), out var importRow))
                {
                    // Lazy Raw Import: if row doesn't exist in DB, create it now (Stage 1)
                    importRow = new ImportRow
                    {
                        ImportFileId = file.Id,
                        SheetName = sheet.Name,
                        SheetIndex = sheet.Index,
                        RowNumber = excelRow.RowNumber,
                        RowHash = RowHashCalculator.Compute(excelRow.Columns),
                        RawPayloadJson = JsonSerializer.Serialize(excelRow.Columns),
                        Status = ImportRowStatus.Pending,
                        ImportedAt = DateTime.UtcNow
                    };
                    Db.ImportRows.Add(importRow);
                    file.TotalRows++;
                }

                try
                {
                    await rowProcessor(excelRow, importRow, cancellationToken);
                    importRow.Status = ImportRowStatus.Transformed;
                    importRow.TransformedAt = DateTime.UtcNow;
                    transformed++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error transforming row {Row} in file {Files}", excelRow.RowNumber, string.Join(", ", file.LogicalNames));
                    RecordError(importRow, "Transform", "TRANS_ERR", ex.Message);
                    importRow.Status = ImportRowStatus.Failed;
                    failed++;
                }
            }
        }

        // Update file level summary
        file.TransformedRows = transformed;
        file.FailedRows = failed;
        file.Status = failed == 0 ? ImportFileStatus.Completed : ImportFileStatus.PartiallyCompleted;

        return new TransformFileResult(transformed, failed, warnings);
    }

    protected void RecordError(
        ImportRow row,
        string stepName,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null) =>
        ErrorRecorder.Record(
            row,
            stepName,
            TransformationSeverity.Error,
            errorCode,
            message,
            fieldName,
            rawValue);

    protected void RecordWarning(
        ImportRow row,
        string stepName,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null) =>
        ErrorRecorder.Record(
            row,
            stepName,
            TransformationSeverity.Warning,
            errorCode,
            message,
            fieldName,
            rawValue);

    protected string? GetValue<TEnum>(RawRowDto row, TEnum column) where TEnum : struct, Enum
    {
        var index = Convert.ToInt32(column);
        if (index >= 0 && index < row.Values.Count)
        {
            return row.Values[index];
        }
        return null;
    }

    protected int? ParseInt<TEnum>(RawRowDto row, TEnum column) where TEnum : struct, Enum
    {
        var val = GetValue(row, column);
        return int.TryParse(val, out var result) ? result : null;
    }

    protected bool ParseBool<TEnum>(RawRowDto row, TEnum column, string[] trueValues) where TEnum : struct, Enum
    {
        var val = GetValue(row, column)?.ToLower();
        return val != null && trueValues.Contains(val);
    }
}
