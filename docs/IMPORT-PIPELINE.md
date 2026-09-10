# Import i uzupełnianie danych

## Komendy

| Komenda | Zachowanie |
|---|---|
| **sync** | Seed ról i administratora, skan inboxu, zapis RAW, transformacja i generowanie mandatów. |
| **extend** | Pobranie składów kadencji 9 i 10 z API Sejmu, uzupełnienie osób i rekonstrukcja zmian mandatów. |
| **full-sync** | Migracja bazy, następnie sync i extend. Używana przez Docker Compose. |
| **db migrate** | Wyłącznie zastosowanie migracji EF Core; nie seeduje kont ani danych. |

Parametry **--force**, **--no-seed** i **--no-identity-seed** oznaczają odpowiednio ponowne przetworzenie znanych plików, wyłączenie generowania przykładowego pliku dla pustego źródła i jawne pominięcie seedowania administratora.

## Przepływ danych

~~~mermaid
flowchart LR
  A[file-mappings.json i inbox] --> B[PipelineRegistry]
  B --> C[ImportSyncService]
  C --> D[ExcelProcessor]
  D --> E[ImportFile i ImportRow RAW]
  E --> F[TransformationExecutor]
  F --> G[SejmModernTransformer]
  G --> H[Encje wyborcze]
  H --> I[MandateGeneratorService]
  J[API Sejmu] --> K[SejmApiClient]
  K --> L[SejmDataExtender]
  L --> M[MandatSuccessionResolver]
~~~

1. **PipelineRegistry** odkrywa transformery oznaczone atrybutem i łączy je z definicjami źródeł.
2. **PipelineContextBuilder** buduje kontekst na podstawie source-data/file-mappings.json.
3. **ImportSyncService** tworzy lub odnajduje jeden ImportBatch na PipelineKey, skanuje pliki i porównuje SHA-256.
4. **ExcelProcessor** otwiera workbook i normalizuje arkusze do modelu pośredniego.
5. Import RAW zapisuje ImportFile oraz każdy ImportRow wraz z JSON-em źródłowym i hashem.
6. **TransformationExecutor** uruchamia transformer. Błąd wiersza trafia do TransformationErrors i nie zatrzymuje kolejnych.
7. **SejmModernTransformer** tworzy lub rozwiązuje wybory, okręgi, listy, komitety, polityków, partie, starty i wyniki.
8. **MandateGeneratorService** tworzy pierwsze mandaty dla wyników z flagą CzyMandat.
9. **ImportReportService** zapisuje raport HTML oraz końcowe liczniki pipeline’u.

## Idempotencja i błędy

- ImportBatch jest identyfikowany przez PipelineKey, a plik przez parę batch + SHA-256.
- Znany checksum jest pomijany, chyba że użyto --force.
- Wiersz jest unikalny w ramach pliku przez arkusz i RowNumber.
- Transformacje rozwiązują istniejące encje przez IEntityResolver.
- Zdarzenia mandatowe mają kontrole przed ponownym dodaniem.
- Brak konfiguracji seedowanego administratora zatrzymuje sync przed importem.
- Błędy HTTP podczas extend kończą komendę; nie są maskowane jako częściowy sukces.

Po wykonaniu należy sprawdzić status batcha, FailedRows, TransformationErrors, raport HTML i log Serilog.

## Usługi uzupełniające

**EntityResolver** wyszukuje istniejące encje i ogranicza duplikaty między rocznikami. **ClubMembershipService** aktualizuje przynależność partyjną w kontekście wyborów.

**DHondtMandateAllocationService** oblicza podział mandatów, a **MandateGeneratorService** materializuje Mandat i początkowe ZdarzenieMandatowe.

**SejmApiClient** pobiera metadane kadencji oraz listy posłów z https://api.sejm.gov.pl/sejm/. Worker obsługuje obecnie kadencje IX i X; błąd HTTP kończy operację.

**SejmDataExtender** dopasowuje członków Sejmu po imieniu i nazwisku, uzupełnia dane osoby i startu oraz rejestruje nieaktywność mandatu. Wiele dopasowań jest oznaczane w InformacjeDodatkowe zamiast arbitralnego wyboru.

**MandatSuccessionResolver** po wygaśnięciu mandatu wybiera kolejnego niewybranego kandydata z tej samej listy: malejąco po liczbie głosów i rosnąco po numerze na liście. Kandydat musi występować w składzie z API. Ograniczenia opisuje [SEJM-IMPORT-AND-MANDATES.md](SEJM-IMPORT-AND-MANDATES.md).

## Konfiguracja i operacje

Inbox domyślnie znajduje się w **source-data/inbox**, logi w **logs/imports**, a raporty w **reports**. Ścieżki nadpisuje sekcja Import. Przed replay lub --force należy wykonać backup i ocenić idempotencję transformera. Szczegóły: [architecture/11-operations-import-backup-replay.md](architecture/11-operations-import-backup-replay.md).
