# Schemat bazy (MariaDB, schema `app`)

Migracje EF w `src/PoliticalPaths.Infrastructure/Persistence/Migrations/`:

| Migracja | Zawartość |
|----------|-----------|
| `InitialImport` | warstwa ETL (import) |
| `AddPipelineKey` | `ImportBatches.PipelineKey`, `LastSyncedAt` |
| `DomainModelSkeleton` | **pełny szkielet domeny** wg [architecture/05-domain-model.md](architecture/05-domain-model.md) i [10-mandate-lifecycle.md](architecture/10-mandate-lifecycle.md) |

Po `db migrate` wszystkie tabele poniżej **istnieją**, ale są **puste** — dane trafiają dopiero z transformerów / seedów.

## Warstwa importu (ETL)

| Tabela | Encja |
|--------|--------|
| `ImportBatches` | batch per pipeline |
| `ImportFiles` | plik Excel w batchu |
| `ImportRows` | wiersz RAW (`RawPayloadJson`) |
| `TransformationErrors` | błędy transformacji wiersza |
| `ImportJobs` | **nieużywane** na dev — szkielet pod ewentualny cron; `sync` ich nie tworzy |

## Geografia

| Tabela | Encja |
|--------|--------|
| `TerritorialUnits` | TERYT (hierarchia) |
| `ElectoralDistrictTerritories` | okręg ↔ TERYT (M:N) |

## Wybory, okręgi, listy

| Tabela | Encja |
|--------|--------|
| `Elections` | kontekst wyborów (rok, izba, profil KW) |
| `ElectoralDistricts` | okręg per wybory |
| `ElectoralDistrictSnapshots` | ludność / uprawnieni / mandaty w okręgu (wersjonowane) |
| `ElectoralCommittees` | komitet wyborczy na dane wybory |
| `ElectoralLists` | lista w okręgu (Sejm / sejmik; nie Senat) |

## Politycy i tożsamość

| Tabela | Encja |
|--------|--------|
| `Politicians` | osoba |
| `PoliticianAliases` | zmiana nazwiska / warianty |
| `PoliticianMergeOverrides` | ręczne scalenie duplikatów |
| `IdentityMatchCandidates` | propozycje dopasowania (fuzzy) |

## Partie i kluby

| Tabela | Encja |
|--------|--------|
| `Parties` | partia (podmiot trwały) |
| `PartyAffiliations` | przynależność w czasie |
| `ParliamentaryClubs` | klub w kadencji |
| `ClubMemberships` | członkostwo w klubie w czasie |

## Starty i wyniki wyborów

| Tabela | Encja |
|--------|--------|
| `Candidacies` | start (`ElectionProfile`, opcjonalna lista) |
| `CandidacyVoteResults` | głosy na kandydata |
| `ElectoralListVoteResults` | głosy na listę w okręgu |
| `DistrictTurnoutResults` | frekwencja / sumy okręgu |

## Mandaty i kadencja

| Tabela | Encja |
|--------|--------|
| `LegislativeTerms` | kadencja Sejmu / Senatu / sejmiku |
| `ElectionMandateAllocations` | przydział mandatu po wyborach |
| `Mandates` | faktyczne pełnienie mandatu (ValidFrom/To) |
| `MandateEvents` | audyt (ślubowanie, wygaśnięcie, …) |

## Mapowania ręczne

| Tabela | Encja |
|--------|--------|
| `ManualMappings` | TERYT / okręg / polityk — ręczne dopasowanie źródła |

## Demo z danymi

Pipeline **`sejm-demo-2023`** po F5 wypełnia większość tabel domenowych (patrz [DEVELOPER-GUIDE.md](DEVELOPER-GUIDE.md) — sekcja „Demo pełnej domeny”).

## Co dalej (kod, nie migracja)

- Kolejne transformery produkcyjne (prawdziwe pliki PKW) — wzoruj się na `SejmDemo2023Transformer`.
- Walidacja profilu wyborów (`Sejm` vs `Senat`) — [09-domain-model-validation-kw.md](architecture/09-domain-model-validation-kw.md).
- Encje w `src/PoliticalPaths.Domain/` — foldery: `Elections`, `Geography`, `Politicians`, `Parties`, `Candidacies`, `Results`, `Mandates`, `Mapping`.
