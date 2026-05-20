# ADR-001: Immutable source-data

## Context

Dane wyborcze pochodzą z wielu plików Excel o różnej jakości i wersjach. Potrzebny audyt i możliwość powtórzenia importu.

## Decision

Katalog `source-data/` jest source of truth. Pliki po dodaniu nie są modyfikowane. Nazwa zawiera `sha8` i `logical-name`. Korekty = nowy plik + nowy batch.

## Consequences

- Git LFS dla dużych plików.
- W bazie przechowujemy `Sha256`, `StoragePath`, `LogicalName` — nie kopiujemy treści Excela do DB poza RAW.
