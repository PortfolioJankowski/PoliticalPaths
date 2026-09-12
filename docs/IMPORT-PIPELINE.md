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

## Dwa źródła opisujące dwa różne momenty

Import danych sejmowych jest dwuetapowy, ponieważ PKW i API Sejmu odpowiadają na inne pytania.

| Źródło | Pytanie, na które odpowiada | Dane |
|---|---|---|
| **Arkusze PKW** | Kto kandydował i jaki był wynik w dniu wyborów? | Wybory, okręgi i ich statystyki, komitety, listy, miejsca na listach, kandydaci, partie, głosy oraz CzyMandat. |
| **API Sejmu** | Kto występował w składzie danej kadencji i jakie informacje publikuje Kancelaria Sejmu? | Dane biograficzne, kontaktowe, zawód, wykształcenie, klub oraz pola InactiveCause i InactiveReason. |

PKW jest źródłem prawdy dla startów i wyniku głosowania. Arkusz nie opisuje późniejszego przebiegu kadencji. Nie wynika z niego, że poseł po roku zrzekł się mandatu, zmarł albo utracił mandat z innej przyczyny. Nie wskazuje też bezpośrednio osoby, która faktycznie weszła do Sejmu na zwolnione miejsce. Bez drugiego etapu baza znałaby wyłącznie skład wybrany w dniu głosowania, a nie zmiany zachodzące podczas kadencji.

## Szczegółowa logika rozszerzania z API Sejmu

Komenda **extend** jest dziś uruchamiana dla kadencji IX i X.

1. **SejmApiClient** pobiera metadane kadencji z endpointu term{n} oraz listę posłów z term{n}/MP. Nieudana odpowiedź HTTP przerywa operację.
2. **SejmDataExtender** wybiera z lokalnej bazy polityków, którzy według PKW uzyskali mandat w wyborach do Sejmu w odpowiedniej kadencji. Wstępny filtr nazwisk ogranicza ilość wczytywanych danych.
3. Każdy polityk jest dopasowywany do listy API przez dokładne, nieczułe na wielkość liter porównanie imienia i nazwiska.
4. Brak dopasowania pozostawia rekord bez zmian. Jedno dopasowanie pozwala kontynuować. Wiele dopasowań nie jest rozstrzygane automatycznie: warianty są zapisywane w InformacjeDodatkowe do późniejszej kontroli.
5. Przy jednoznacznym dopasowaniu data i miejsce urodzenia oraz e-mail aktualizują Polityka. Zawód i wykształcenie aktualizują StartWyborczy w konkretnej kadencji, ponieważ mogą zmieniać się w czasie.
6. Jeżeli API nie zwraca InactiveCause, mandat pozostaje aktywny i procedura dla tej osoby kończy się.
7. Jeżeli InactiveCause występuje, system odnajduje Mandat utworzony z właściwego StartuWyborczego i zmienia jego status na Wygasniety.
8. Przyczyna „Zrzeczenie” tworzy zdarzenie Zrzeczenie, „Zgon” tworzy Zgon, a inne wartości są mapowane na ogólne Wygasniecie. InactiveReason staje się opisem. Przed dodaniem wykonywana jest kontrola, czy mandat nie ma już zdarzenia tego typu.
9. Nowe zdarzenie wygaśnięcia uruchamia **MandatSuccessionResolver**.
10. Resolver znajduje listę wyborczą poprzedniego posła i pobiera kandydatów z tej samej listy, którzy według PKW nie uzyskali mandatu oraz nie mają już mandatu w tej kadencji.
11. Kandydaci są sortowani malejąco po liczbie głosów, a przy remisie rosnąco po pozycji na liście.
12. Resolver sprawdza tę kolejność względem listy członków Sejmu z API. Pierwszy pasujący kandydat jest traktowany jako następca; jego dane są uzupełniane, a system tworzy Mandat typu Sukcesja i zdarzenie Wstąpienie.

### Dlaczego następca musi istnieć w obu źródłach?

PKW pozwala wyznaczyć kolejność niewybranych kandydatów z tej samej listy, ale sama kolejność nie dowodzi objęcia mandatu. Kandydat może nie skorzystać z pierwszeństwa. Obecność w danych API Sejmu jest dodatkowym sygnałem, że dana osoba rzeczywiście znalazła się w składzie kadencji. Mechanizm łączy więc wynik wyborczy z późniejszym składem zamiast automatycznie przyznawać mandat każdej kolejnej osobie z listy.

### Ograniczenia obecnej implementacji

- Dopasowanie po imieniu i nazwisku nie jest odpornym identyfikatorem osoby. Kolizje wymagają weryfikacji.
- API dostarcza przyczynę i opis nieaktywności, lecz obecny DTO nie daje wiarygodnej urzędowej daty zdarzenia.
- Data wygaśnięcia jest dziś technicznie ustawiana na dzień po rozpoczęciu dotychczasowego mandatu.
- Data sukcesji jest dziś ustawiana na dzień po początku kadencji, a nie na rzeczywisty dzień objęcia mandatu.
- System nie modeluje zawiadomienia Marszałka Sejmu, odmowy przyjęcia pierwszeństwa ani dokumentu urzędowego potwierdzającego zmianę.
- Wynik należy traktować jako rekonstrukcję analityczną. Dokładne daty i podstawy prawne wymagają potwierdzenia w dokumentach urzędowych.

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
