# ETL dwuetapowy

## Etap 1 — RAW IMPORT

**Cel:** jak najbliżej pliku Excel + pełny audyt.

### Co zapisujemy

| Element | Opis |
|---------|------|
| `ImportBatch` | jedna „dostawa” plików |
| `ImportFile` | metadane pliku: ścieżka, SHA, logical name, status |
| `ImportRow` | jeden wiersz arkusza: `SheetName`, `RowNumber`, `RawPayloadJson`, `RowHash` |
| Opcjonalnie `Raw_*` | dedykowane tabele per rodzina formatów (denormalizowane kolumny + techniczne) |

### Zasady RAW

- Bez pełnej normalizacji domenowej.
- Wartości oryginalne (string) w JSON — nawet jeśli później parsujesz do int/dat.
- `RowHash` = hash(normalized payload) — idempotency wiersza.
- Numer arkusza i wiersza — śledzenie w logach i `TransformationError`.

### Kiedy dedykowana tabela `Raw_*`

- Format używany często i stabilny — łatwiejsze SQL debugowe.
- Rzadkie / eksperymentalne formaty — tylko `ImportRow`.

## Etap 2 — TRANSFORM

**Cel:** model domenowy (`Politician`, `Candidacy`, `VoteResult`, …).

### Wejście

Wiersze `ImportRow` ze statusem `Pending` (lub `Failed` przy resume).

### Wyjście

- Zaktualizowane encje domenowe.
- `ImportRow.Status` → `Transformed` | `Skipped` | `Failed` | `NeedsManualReview`.
- `TransformationError` per wiersz / krok.

### Transakcje

| Granica | Zalecenie |
|---------|-----------|
| RAW import jednego pliku | jedna transakcja DB |
| Transform | batche po N wierszach (np. 100–500) — jedna transakcja na batch |
| Pojedynczy wiersz | logika w transformerze; błąd krytyczny → rollback batcha lub tylko ten wiersz `Failed` (konfigurowalne) |

### Replay

1. Zostaw RAW (`ImportRow`) bez zmian.
2. Ustaw wiersze na `Pending` (lub nowy batch transform-only).
3. Uruchom ten sam transformer (po poprawce kodu).

### Statusy pliku

```
Discovered → RawImporting → RawCompleted → Transforming → Completed
                                                      → PartiallyCompleted
                                                      → Failed
```

## Orkiestracja (skrót)

Patrz [06-observability.md](06-observability.md) i [implementation-plan.md](../implementation-plan.md).

- **Idempotency:** SHA pliku + `RowHash`.
- **Resume:** `ImportFile.LastProcessedRowId` lub status per `ImportRow`.
- **Retry:** Polly na błędy przejściowe (sieć, DB timeout).
- **Scheduler:** Hangfire — skan katalogu / manifest / ręczny trigger CLI.
