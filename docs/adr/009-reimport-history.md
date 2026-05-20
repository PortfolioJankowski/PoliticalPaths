# ADR-009: Reimport z zachowaniem historii

## Decision

Reimport = nowy `ImportBatch`, opcjonalnie `SupersedesBatchId`. Ten sam SHA pliku → domyślnie skip (idempotency). Stare batche i fakty nie są usuwane automatycznie.

## Consequences

Baza rośnie — potrzebna polityka archiwizacji w przyszłości; za to pełna reprodukowalność badań.
