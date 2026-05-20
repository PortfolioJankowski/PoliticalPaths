# Observability — logowanie i encje importu

## Wymagania

- Osobny log **per plik**.
- Status importu, liczba rekordów, błędów, warningów, czas wykonania.
- Śledzenie transformacji **konkretnego wiersza** (`ImportRowId`).
- Structured logging + **CorrelationId** = `ImportBatchId`.

## Serilog

### Sinki

| Sink | Użycie |
|------|--------|
| Console | dev — czytelny template |
| File (Compact JSON) | produkcja / analiza |

Ścieżka:

```
logs/imports/{yyyy}/{MM}/{ImportBatchId}/{ImportFileId}.log
```

### Enrichment

```csharp
LogContext.PushProperty("ImportBatchId", batchId);
LogContext.PushProperty("ImportFileId", fileId);
LogContext.PushProperty("ImportRowId", rowId);
LogContext.PushProperty("LogicalName", logicalName);
LogContext.PushProperty("CorrelationId", batchId);
```

### Pakiety

- `Serilog.AspNetCore`
- `Serilog.Sinks.File`
- `Serilog.Formatting.Compact`

## Encje techniczne

### `ImportBatch`

| Pole | Opis |
|------|------|
| `Id` | GUID, CorrelationId |
| `Status` | Created, Running, RawCompleted, Transforming, Completed, PartiallyCompleted, Failed |
| `ElectionYear` | opcjonalnie |
| `StartedAt` / `CompletedAt` | |
| `TriggeredBy` | manual, scheduler, cli |
| `SupersedesBatchId` | reimport |

### `ImportFile`

| Pole | Opis |
|------|------|
| `LogicalName` | klucz do transformera |
| `StoragePath`, `Sha256`, `FileSizeBytes` | |
| `FormatVersion` | v1, v2 |
| `Status` | Discovered → … → Completed |
| `TotalRows`, `TransformedRows`, `FailedRows`, `WarningCount` | |
| `Duration`, `LogFilePath` | |
| `LastProcessedRowId` | resume |

### `ImportRow`

| Pole | Opis |
|------|------|
| `SheetName`, `SheetIndex`, `RowNumber` | |
| `RawPayloadJson` | oryginał kolumn |
| `RowHash` | idempotency |
| `Status` | Pending, Transformed, Failed, Skipped, NeedsManualReview |
| `DomainEntityType`, `DomainEntityId` | np. Candidacy GUID |

### `TransformationError`

| Pole | Opis |
|------|------|
| `ImportRowId`, `StepName`, `ErrorCode` | |
| `Severity` | Error, Warning |
| `Message`, `FieldName`, `RawValue`, `DetailsJson` | |

### `ImportJob` (orchestracja)

| Pole | Opis |
|------|------|
| `JobType` | RawImport, Transform, FullPipeline |
| `Attempt`, `NextRetryAt`, `LastError` | |

### `ImportLog` (opcjonalna persystencja)

Denormalizacja ważnych zdarzeń z Serilog do tabeli — do UI statusu bez czytania plików.

## Metryki (faza późniejsza)

OpenTelemetry: `import.rows.processed`, `import.duration`, `import.errors`.
