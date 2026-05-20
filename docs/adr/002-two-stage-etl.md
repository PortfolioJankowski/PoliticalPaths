# ADR-002: ETL dwuetapowy RAW → Transform

## Decision

Etap 1: Excel → `ImportRow` (+ opcjonalnie `Raw_*`). Etap 2: ręczne transformery → model domenowy. Replay = reset statusów wierszy + ponowny transform.

## Consequences

Dłuższy czas importu, ale pełny audyt i debugowalność.
