# Schemat bazy danych

Political Paths korzysta z MariaDB 11.4 i Entity Framework Core. Źródłem prawdy są **AppDbContext**, konfiguracje encji oraz snapshot ostatniej migracji w katalogu **src/PoliticalPaths.Infrastructure/Migrations**.

Identyfikatory domenowe i użytkowników są przechowywane jako char(36), daty bez czasu jako date, znaczniki czasu importu jako datetime(6), wartości logiczne jako tinyint(1), a enumy jako int. MariaDB nie używa osobnych schematów logicznych.

## Diagram relacji

~~~mermaid
erDiagram
  ImportBatches ||--o{ ImportFiles : zawiera
  ImportFiles ||--o{ ImportRows : zawiera
  ImportRows ||--o{ TransformationErrors : rejestruje
  RodzajeWyborow ||--o{ Wybory : klasyfikuje
  Wybory ||--o{ SzczegolyOkregow : opisuje
  OkregWyborczy ||--o{ SzczegolyOkregow : posiada
  Wybory ||--o{ ListaWyborcza : posiada
  ListaWyborcza ||--o{ StartyWyborcze : grupuje
  Politycy ||--o{ StartyWyborcze : startuje
  Wybory ||--o{ StartyWyborcze : obejmuje
  WynikiWyborow ||--|| StartyWyborcze : wynik
  Politycy ||--o{ PartieCzlonkostwa : posiada
  Partia ||--o{ PartieCzlonkostwa : obejmuje
  StartyWyborcze ||--o{ Mandaty : prowadzi_do
  Politycy ||--o{ Mandaty : sprawuje
  Mandaty ||--o{ ZdarzeniaMandatowe : historia
  AspNetUsers ||--o{ AspNetUserRoles : ma
  AspNetRoles ||--o{ AspNetUserRoles : przypisana
~~~

Nie wszystkie identyfikatory logiczne są obecnie fizycznymi FK. Część powiązań list i komitetów jest egzekwowana przez transformery; należy to uwzględnić przy ręcznych modyfikacjach.

## Warstwa importu

| Tabela | Klucz i najważniejsze kolumny | Relacje, indeksy i przeznaczenie |
|---|---|---|
| **ImportBatches** | Id PK; PipelineKey varchar(128); Status; PrimarySourceType; ElectionYear?; StartedAt; CompletedAt?; LastSyncedAt?; TriggeredBy?; Notes?; SupersedesBatchId? | Unikalny PipelineKey; indeksy czasu i statusu. Jeden trwały batch na pipeline. |
| **ImportFiles** | Id PK; ImportBatchId FK; LogicalNames; StoragePath; Sha256 varchar(64); rozmiar, format, status, liczniki i czasy RAW | Cascade z batcha; indeks (ImportBatchId, Sha256) służy idempotencji. |
| **ImportRows** | Id bigint PK auto; ImportFileId FK; arkusz, numer i hash wiersza; RawPayloadJson; status; czasy; opcjonalny typ i ID encji docelowej | Cascade z pliku; unikalny (ImportFileId, SheetName, RowNumber) i indeks statusu. Niezmienny zapis RAW. |
| **TransformationErrors** | Id bigint PK auto; ImportRowId FK; krok, severity, kod, komunikat, pole, wartość RAW, szczegóły JSON i czas | Cascade z wiersza; wiele błędów może wskazywać jeden wiersz. |

## Model wyborczy i politycy

| Tabela | Klucz i kolumny | Znaczenie |
|---|---|---|
| **RodzajeWyborow** | Id PK; Nazwa varchar(100); Poziom | Słownik rodzaju i poziomu wyborów. |
| **Wybory** | Id PK; RodzajWyborowId FK; DataOgloszenia?; DataWyborow; Kadencja?; Ordynacja; Tura; CzyPrzedterminowe | Jedno wydarzenie wyborcze. |
| **OkregWyborczy** | Id PK; NumerOkregu; RodzajWyborowId | Tożsamość okręgu dla rodzaju wyborów. |
| **SzczegolyOkregow** | złożony PK (OkregId, WyboryId); rok, mieszkańcy, uprawnieni, liczba mandatów/list/kandydatów | Snapshot okręgu; FK do okręgu i wyborów. |
| **KomitetyWyborcze** | Id PK; Nazwa; Skrot? | Komitet zgłaszający listę lub kandydata. |
| **ListaWyborcza** | Id PK; OkregId; NumerListy; WyboryId; KomitetWyborczyId | Lista w konkretnych wyborach i okręgu. |
| **Politycy** | Id PK; imiona i nazwisko; data/miejsce urodzenia; e-mail; informacje dodatkowe | Kanoniczny rekord osoby łączony pomiędzy wyborami. |
| **Partia** | Id PK; Nazwa; Skrot?; daty rozpoczęcia i zakończenia działalności | Trwały podmiot partyjny. |
| **PartieCzlonkostwa** | Id PK; PolitykId, PartiaId, WyboryId FK; IsActive | Przynależność w kontekście wyborów; FK mają cascade. |
| **WynikiWyborow** | Id PK; LiczbaGlosow; CzyMandat | Wynik kandydata powiązany ze startem. |
| **StartyWyborcze** | Id PK; PolitykId FK; ListaId? FK; pozycja, zawód, wykształcenie, miejsce; PartiaId?; KomitetId; WynikiId FK; PopierajacaPartiaId?; WyboryId FK | Fakt kandydowania. Usunięcie listy jest Restrict; główne FK domenowe używają Cascade. |

## Mandaty

| Tabela | Klucz i kolumny | Znaczenie |
|---|---|---|
| **Mandaty** | Id PK; PolitykId FK; StartWyborczyId FK; DataOd; Status; TypObjecia | Faktyczne objęcie mandatu. Usunięcie startu jest Restrict. |
| **ZdarzeniaMandatowe** | Id bigint PK auto; MandatId i PolitykId FK; Typ; DataZdarzenia; Opis?; DokumentReferencyjny? | Historia wyboru, wstąpienia, zrzeczenia, zgonu lub wygaśnięcia. |

StatusMandatu: Aktywny, Wygasniety, Zakonczony. TypObjeciaMandatu: WyborBezposredni, Sukcesja. TypZdarzeniaMandatowego: Wybor, Wstapienie, Zrzeczenie, Zgon, Wygasniecie, KoniecKadencji.

## ASP.NET Core Identity

| Tabela | Przeznaczenie |
|---|---|
| **AspNetUsers** | E-mail/login, hash hasła, security stamp, lockout oraz własne flagi IsActive i MustChangePassword. |
| **AspNetRoles** | Role Admin i User; unikalny NormalizedName. |
| **AspNetUserRoles** | Złożony PK (UserId, RoleId) i FK cascade. |
| **AspNetUserClaims / AspNetRoleClaims** | Opcjonalne claims użytkowników i ról. |
| **AspNetUserLogins** | Loginy zewnętrzne; obecnie niewykorzystywane. |
| **AspNetUserTokens** | Tokeny Identity, w tym obsługa resetu hasła. |

NormalizedUserName jest unikalny, a NormalizedEmail indeksowany. Aplikacja wymaga unikalnego e-maila przez UserManager.

## Migracje i weryfikacja

~~~powershell
dotnet run --project src/PoliticalPaths.ImportWorker -- db migrate
dotnet ef migrations list --project src/PoliticalPaths.Infrastructure --startup-project src/PoliticalPaths.ImportWorker
dotnet ef migrations script --idempotent --project src/PoliticalPaths.Infrastructure --startup-project src/PoliticalPaths.ImportWorker
~~~

Przed migracją produkcyjną należy wykonać backup. Migracja **Initial** tworzy model importowy i domenowy, a **AddIdentity** dodaje wyłącznie tabele kont. Dokument aktualizujemy razem z każdą migracją.
