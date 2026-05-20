# ADR-004: Ręczne transformery + atrybut LogicalNames

## Context

Będzie **bardzo dużo** plików o **różnych** strukturach. Automatyczne mapowanie kolumn nie skaluje się. Część plików współdzieli logikę (np. Sejm 2019 vs 2023).

## Decision

1. Każdy transformer to **ręcznie pisana** klasa `IImportTransformer`.
2. Rejestracja: atrybut `[ImportTransformer(LogicalNames = [...])]` na klasie.
3. Przy starcie: skan assembly → słownik `logicalName → Type`; kolizje = fail fast.
4. Import wiersza = sekwencja domenowa (polityk → start → fakty), nie bulk do jednej tabeli.
5. Współdzielona logika w klasach bazowych / serwisach `TransformContext`, nie w kopiowaniu całych transformerów.

## Consequences

- Dodanie pliku = (zwykle) nowa klasa lub rozszerzenie `LogicalNames` w atrybucie.
- Trzeba utrzymać słownik `logical-name` w `source-data/README.md` zsynchronizowany z atrybutami.
- Testy: golden rows (JSON) per transformer.
