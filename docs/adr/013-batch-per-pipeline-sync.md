# ADR-013: Jeden batch per pipeline (transformer), synchronny full import

## Context

Developer pracuje z wieloma typami plików / transformerami. Model „jeden F5 = jeden batch ze wszystkimi plikami z inbox” tworzy szum i nie odpowiada mapowaniu 1 transformer = 1 obszar danych.

Hangfire i `ImportJob` nie są potrzebne na etapie dev — import ma być synchroniczny (F5).

## Decision

1. **`ImportBatch` = jeden pipeline** (klucz transformera / logical scope), nie jedno przypadkowe uruchomienie.
2. **`PipelineKey`** (np. `sejm-2023-listy`) — stabilny, UNIQUE; batch get-or-create po kluczu.
3. **Uruchomienie `sync`/`dev`**: dla każdego zarejestrowanego pipeline — skan folderu → porównanie SHA z `ImportFile` w batchu → brakujące = full pipeline (RAW + transform), istniejące = skip (lub `--force`).
4. **Inbox per pipeline**: `source-data/inbox/{pipeline-key}/*.xlsx`
5. **`ImportJob` / Hangfire** — poza zakresem dev; ewentualnie tylko gdy kiedyś będzie nocny cron (opcjonalnie).
6. **MediatR** — dopuszczalne na start; można zastąpić zwykłym `ImportSyncService` gdy rośnie złożoność — nie blokuje.

## Consequences

- Mniej rekordów `ImportBatch` w DB, czytelna historia per typ danych.
- Wspólny transformer dla wielu logical names (Sejm 2019 + 2023) → **jeden** batch, **wiele** `ImportFile`.
- Refaktor Fazy 1 inbox: z flat inbox na podfoldery (planowane).
