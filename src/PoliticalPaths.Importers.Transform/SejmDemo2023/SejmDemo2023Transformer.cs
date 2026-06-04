using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

/// <summary>
/// Demo end-to-end: wiele arkuszy → pełny model domenowy (wybory, okręgi, listy, kandydatury, wyniki, mandaty, kluby).
/// </summary>
public sealed class SejmDemo2023Transformer(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger<SejmDemo2023Transformer> logger)
    : ExcelFileTransformerBase(db, errorRecorder, logger)
{
    public override string PipelineKey => "sejm-demo-2023";

    public override async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // 1. Load ImportRows from DB (synced previously in Stage 1)
        var rows = await Db.ImportRows
            .Where(r => r.ImportFileId == file.Id)
            .ToListAsync(cancellationToken);

        var rowsMap = rows.ToDictionary(r => (r.SheetName, r.RowNumber));

        var transformed = 0;
        var failed = 0;
        var warnings = 0;

        foreach (var sheet in workbook.Sheets)
        {
            foreach (var excelRow in sheet.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Find matching ImportRow for error recording and status updates
                if (!rowsMap.TryGetValue((sheet.Name, excelRow.RowNumber), out var importRow))
                {
                    // This shouldn't happen if Stage 1 was correct, but let's be safe
                    Logger.LogWarning("ImportRow not found for {Sheet} Row {Row}", sheet.Name, excelRow.RowNumber);
                    continue;
                }

            }
        }

        await Db.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Sejm demo transform file {FileId}: transformed={Transformed}, failed={Failed}, warnings={Warnings}",
            file.Id,
            transformed,
            failed,
            warnings);

        return new TransformFileResult(transformed, failed, warnings);
    }

}
