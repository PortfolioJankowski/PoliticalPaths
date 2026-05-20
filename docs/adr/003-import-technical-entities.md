# ADR-003: Encje techniczne importu

## Decision

Obowiązkowe: `ImportBatch`, `ImportFile`, `ImportRow`, `TransformationError`. Opcjonalnie: `ImportJob`, `ImportLog`.

## Consequences

Każda operacja domenowa w transformacji linkuje `SourceImportRowId` / `ImportBatchId` gdzie to możliwe.
