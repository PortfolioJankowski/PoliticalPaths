# PoliticalPaths — dokumentacja

System do analizy ścieżek karier politycznych w Polsce.

## Spis treści

| Dokument | Opis |
|----------|------|
| **[DEVELOPER-GUIDE.md](DEVELOPER-GUIDE.md)** | **F5, inbox, ImportBatch, gdzie pisać transformery** |
| [architecture/01-overview.md](architecture/01-overview.md) | Cele, założenia, przepływ danych |
| [architecture/02-source-data.md](architecture/02-source-data.md) | Katalog `/source-data`, naming, immutable SOT |
| [architecture/03-etl-two-stage.md](architecture/03-etl-two-stage.md) | RAW import → transform, replay, reimport |
| [architecture/04-transformers.md](architecture/04-transformers.md) | **Ręczne transformery**, atrybuty, wiele plików → jeden transformer |
| [architecture/05-domain-model.md](architecture/05-domain-model.md) | **Model domenowy** — okręgi, TERYT, snapshoty, listy, wyniki |
| [architecture/09-domain-model-validation-kw.md](architecture/09-domain-model-validation-kw.md) | **Walidacja modelu** względem Kodeksu wyborczego |
| [architecture/10-mandate-lifecycle.md](architecture/10-mandate-lifecycle.md) | **Mandat i kadencja** — wygaśnięcie, sukcesja, oddzielenie od wyniku wyborów |
| [architecture/11-operations-import-backup-replay.md](architecture/11-operations-import-backup-replay.md) | **Operacje** — co pokazuje UI, API mandatów, backup, idempotency |
| [architecture/06-observability.md](architecture/06-observability.md) | Serilog, encje importu, logi per plik |
| [architecture/07-solution-structure.md](architecture/07-solution-structure.md) | Projekty solution, CQRS, joby |
| [architecture/08-libraries-and-graphql.md](architecture/08-libraries-and-graphql.md) | NuGet, Excel, GraphQL (przyszłość) |
| [implementation-plan.md](implementation-plan.md) | **Plan kroków implementacyjnych** (iteracje) |
| [adr/README.md](adr/README.md) | Architecture Decision Records |

## Model domenowy — skrót

Szczegóły: **[architecture/05-domain-model.md](architecture/05-domain-model.md)**.

| Pojęcie | Reguła |
|---------|--------|
| **Okręg** | Zawsze w kontekście **`Election`** i **`ElectoralChamber`** (Sejm ≠ Senat ≠ sejmik). Numer 12 w 2019 ≠ numer 12 w 2023. |
| **TERYT** | Słownik `TerritorialUnit`; okręg łączy się **M:N** (województwo / powiat / miasto). Start polityka = okręg wyborczy, nie pojedynczy kod TERYT. |
| **Statystyki okręgu** | `ElectoralDistrictSnapshot` (mieszkańcy, uprawnieni, …) — **wersjonowane per wybory**, bez nadpisywania. |
| **Lista** | `ElectoralList` w okręgu — **Sejm i sejmik**; w **Senacie brak list** (głos na kandydata). |
| **Komitet** | `ElectoralCommittee` zgłasza listę/kandydata (KW ≠ to samo co `Party`). |
| **Start** | `Candidacy` = polityk + wybory + okręg (+ lista **lub** komitet, zależnie od profilu wyborów). |
| **Wyniki** | Osobne encje z `ElectionId`; inny rok = inne rekordy. |
| **KW** | Szczegóły zgodności prawnej → [09-domain-model-validation-kw.md](architecture/09-domain-model-validation-kw.md). |
| **Mandat w czasie** | `Mandate` + `LegislativeTerm` — kto faktycznie sprawował urząd; `Elected` to tylko wynik wyborów → [10-mandate-lifecycle.md](architecture/10-mandate-lifecycle.md). |

## Zasady projektu (skrót)

- Wieloletni rozwój, zmiana formatów Excel, nieidealna jakość danych.
- Częściowo ręczne mapowania (TERYT, okręgi, identity polityków).
- Reimport bez utraty historii (`ImportBatch`, `SupersedesBatchId`).
- Import to **złożona operacja domenowa** (wybory → okręg → lista → polityk → start → wyniki), nie proste `INSERT` do jednej tabeli.
- Transformery pisane **ręcznie**; jeden transformer może obsługiwać **wiele** plików/formatów (rejestracja przez atrybut).

## Stan repozytorium

Dokumentacja opisuje docelową architekturę. Kod jest na etapie szkieletu — realizacja według [implementation-plan.md](implementation-plan.md).
