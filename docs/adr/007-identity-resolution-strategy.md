# ADR-007: Identity resolution polityków

## Decision

Pipeline: manual override → twarde ID → birth date + name → fuzzy score. Zmiana nazwiska = `PoliticianAlias`. Partie/kluby = osobne tabele z `ValidFrom`/`ValidTo`.

## Consequences

Wymaga okresowego ręcznego review; `NeedsManualReview` na wierszach importu.
