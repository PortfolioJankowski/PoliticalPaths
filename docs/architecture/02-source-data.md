# Source of Truth — `/source-data`

## Zasada

Pliki w `source-data/` są **immutable**. Po dodaniu do repozytorium nie są edytowane. Korekta danych = nowy plik + nowy `ImportBatch`. Stare batche pozostają w historii.

## Struktura katalogów

```
source-data/
  README.md
  manifest/                              # opcjonalnie
    {import-batch-id}.json
  {rok}/
    {typ-wyborow}/                      # sejm | senat | prezydenckie | samorzadowe | europarlament
      {organ}/                          # sejm | senat | …
        {etap}/                          # listy | wyniki-okreg | glosy | teryt | …
          v{format-version}/            # v1, v2 — przy zmianie układu kolumn
            {sha8}_{nazwa-logiczna}_{yyyy-MM-dd}.xlsx
```

### Przykład

```
source-data/2023/sejm/sejm/listy/v2/b91e0042_sejm-2023-listy-kandydatow_2023-10-15.xlsx
```

## Naming convention

| Segment | Reguła |
|---------|--------|
| `rok` | 4 cyfry |
| `typ-wyborow` | kebab-case ze słownika `ElectionType` |
| `organ`, `etap` | ze słownika dokumentowanego w `source-data/README.md` |
| `format-version` | `v{n}` — inkrement przy zmianie struktury Excela |
| nazwa pliku | `{sha8}_{logical-name}_{data-publikacji}.xlsx` |

`sha8` — pierwsze 8 znaków SHA-256 **oryginalnego** pliku (przed kopią do repo).

## Logical name

**Logical name** (`sejm-2023-listy-kandydatow`) to stabilny identyfikator scenariusza importu. Służy do:

- wyboru transformera w rejestrze (atrybut na klasie),
- manifestów,
- logów i raportów.

Nie musi być równy nazwie pliku na dysku — mapowanie: `ImportFile.LogicalName`.

## Git LFS

Duże `.xlsx` (> ~5 MB) — Git LFS. W bazie: `StoragePath`, `Sha256`, `FileSizeBytes`.

## Manifest (opcjonalny)

```json
{
  "batchId": "0192a3b4-cdef-7890-abcd-ef1234567890",
  "electionYear": 2023,
  "files": [
    {
      "logicalName": "sejm-2023-listy-kandydatow",
      "relativePath": "2023/sejm/sejm/listy/v2/b91e0042_sejm-2023-listy-kandydatow_2023-10-15.xlsx",
      "sha256": "full-hash-here",
      "formatVersion": "v2"
    }
  ]
}
```

## Reimport

| Tryb | Zachowanie |
|------|------------|
| Nowy plik (inny SHA) | Nowy `ImportBatch`, pełny RAW + transform |
| Ten sam SHA | Idempotency — skip lub jawny `ForceReimport` |
| Korekta domeny po transform | Nowy batch z `SupersedesBatchId` wskazującym poprzedni |

Historia importów i starych wersji faktów domenowych **nie jest usuwana** bez jawnej polityki archiwizacji.
