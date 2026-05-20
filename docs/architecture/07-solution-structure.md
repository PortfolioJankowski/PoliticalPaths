# Struktura solution

## Docelowa struktura

```
PoliticalPaths/
├── src/
│   ├── PoliticalPaths.Api/              # Minimal API; GraphQL później
│   ├── PoliticalPaths.Application/      # MediatR, orchestracja, walidacja
│   ├── PoliticalPaths.Domain/           # encje, enums, porty
│   ├── PoliticalPaths.Infrastructure/   # EF Core, MariaDB, Serilog, Hangfire
│   ├── PoliticalPaths.Importers/
│   │   ├── PoliticalPaths.Importers.Raw/
│   │   └── PoliticalPaths.Importers.Transform/   # ręczne transformery + atrybuty
│   ├── PoliticalPaths.ImportWorker/     # host jobów / CLI
│   └── PoliticalPaths.Shared/           # normalizacja nazw, helpery
├── tests/
│   ├── PoliticalPaths.Domain.Tests/
│   ├── PoliticalPaths.Importers.Tests/
│   └── PoliticalPaths.Integration.Tests/
├── source-data/
├── docs/
└── PoliticalPaths.sln
```

## CQRS + MediatR

| Użyj | Nie używaj na |
|------|----------------|
| Commands: `RegisterImportFile`, `RunRawImport`, `RunTransform` | Prosty healthcheck |
| Queries: status batcha, lista błędów wiersza | |
| Pipeline behaviors: logging, FluentValidation | |

Logika transformacji zostaje w **`IImportTransformer`** — MediatR tylko **uruchamia** pipeline, nie zastępuje transformerów.

## FluentValidation

- Commands (ścieżka pliku, logical name, batch id).
- DTO ręcznych mapowań (`ManualMapping`).

## Background jobs

`PoliticalPaths.ImportWorker`:

- Hangfire — `FullImportPipeline`, `TransformOnly`, `RetryFailedRows`.
- Alternatywa na start: `IHostedService` + kolejka w DB bez Hangfire (faza 0–1).

## Konwencja nazw transformerów

```
{Obszar}{Rok}{Opis}Transformer.cs
SejmDistrictResultsTransformer.cs      // wspólny dla wielu lat — atrybut z wieloma LogicalNames
Sejm2023CandidateListsTransformer.cs   // gdy logika mocno odbiega
```
