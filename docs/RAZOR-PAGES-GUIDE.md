# Razor Pages w Political Paths

Dokument opisuje Razor Pages oraz rozwiązania użyte w PoliticalPaths.Dashboard.

## 1. Struktura strony

Każda strona jest zwykle parą plików:

    Pages/Politicians/Index.cshtml
    Pages/Politicians/Index.cshtml.cs

Plik .cshtml zawiera HTML i składnię Razor. Plik .cshtml.cs zawiera klasę PageModel, czyli logikę strony.

    @page
    @model Politicians.IndexModel
    <h1>@Model.Title</h1>

Dyrektywa @page rejestruje plik jako endpoint HTTP. @model wskazuje klasę code-behind.

    public sealed class IndexModel : PageModel
    {
        public string Title { get; private set; } = "";

        public void OnGet() => Title = "Politycy";
    }

Publiczne właściwości PageModel są dostępne w widoku przez Model.

## 2. Komunikacja widoku z code-behind

Code-behind przygotowuje właściwość, a widok ją wyświetla:

    public List<PoliticianListItem> Politicians { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Politicians = await queries.SearchPoliticiansAsync(Q, 1, 50, ct);
    }

    @foreach (var politician in Model.Politicians)
    {
        <div>@politician.Name</div>
    }

Parametr trasy definiuje się przez:

    @page "{id:guid}"

i odbiera w handlerze:

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Politician = await queries.GetPoliticianAsync(id, ct);
        return Politician is null ? NotFound() : Page();
    }

## 3. Atrybuty w code-behind

[BindProperty] włącza model binding, czyli automatyczne pobieranie danych z requestu:

    [BindProperty]
    public EditModel Input { get; set; } = new();

Dla parametrów GET trzeba dodać SupportsGet:

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

Można zmienić nazwę parametru HTTP:

    [BindProperty(Name = "page", SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

Adres /Politicians?page=2 ustawi PageNumber na 2.

Do walidacji służą np. Required i StringLength:

    [BindProperty]
    [Required(ErrorMessage = "Nazwa jest wymagana")]
    [StringLength(100)]
    public string Name { get; set; } = "";

CancellationToken requestu należy przekazywać do EF Core, aby przerwać zapytanie po zamknięciu strony.

## 4. Formularze i handlery

Formularz wyszukiwania powinien używać GET:

    <form method="get">
        <input name="q" value="@Model.Q" />
        <button type="submit">Szukaj</button>
    </form>

Zmiana danych powinna używać POST:

    <form method="post">
        <input asp-for="Input.Name" />
        <span asp-validation-for="Input.Name"></span>
        <button type="submit">Zapisz</button>
    </form>

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        await service.SaveAsync(Input, ct);
        return RedirectToPage();
    }

ASP.NET Core dodaje token anty-CSRF do formularzy POST.

Jeżeli strona ma więcej operacji GET, używamy named handlers:

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        var bytes = await exporter.ExportAsync(ct);
        return File(bytes, "text/csv", "politycy.csv");
    }

    <a asp-page-handler="Export">Eksportuj CSV</a>

Powstaje adres /Politicians?handler=Export. Analogicznie działają OnPostApproveAsync i OnPostRejectAsync. Nie tworzymy kilku metod o tej samej nazwie OnGet.

## 5. Cykl życia strony

    request HTTP
      -> wybór pliku przez @page
      -> utworzenie PageModel przez DI
      -> model binding parametrów
      -> OnGet lub OnPost
      -> Page, RedirectToPage, NotFound albo File
      -> renderowanie .cshtml
      -> response HTTP

Najczęstsze handlery:

    void OnGet()
    Task OnGetAsync()
    IActionResult OnPost()
    Task<IActionResult> OnPostAsync()

OnGet obsługuje GET, OnPost obsługuje POST, a wariant Async służy do operacji asynchronicznych.

## 6. Tag Helpers asp-*

asp-page tworzy link do strony:

    <a asp-page="/Politicians/Index">Politycy</a>

asp-route-* przekazuje parametr trasy:

    <a asp-page="Details" asp-route-id="@politician.Id">Szczegóły</a>

asp-page-handler wybiera named handler:

    <button asp-page-handler="Export">Eksportuj</button>

asp-append-version dodaje hash do pliku statycznego:

    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />

asp-for wiąże pole formularza z właściwością, a asp-validation-for pokazuje błąd tego pola:

    <input asp-for="Input.Name" />
    <span asp-validation-for="Input.Name"></span>

## 7. DI i warstwa zapytań

Program.cs dashboardu rejestruje:

    builder.Services.AddRazorPages();
    builder.Services.AddApplication();
    builder.Services.AddRawImporters();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<DashboardQueryService>();

PageModel pobiera serwis przez konstruktor:

    public sealed class IndexModel(DashboardQueryService queries) : PageModel

DashboardQueryService wykonuje zapytania EF Core i zwraca DTO. Widok nie powinien sam odpytywać bazy.

Dla zapytań read-only stosujemy AsNoTracking, projekcję Select, paginację Skip/Take, filtrowanie w bazie i CancellationToken.

## 8. Dobre praktyki

- Oddziel prezentację, PageModel, serwisy i dostęp do danych.
- Używaj DTO/ViewModeli zamiast całych encji domenowych.
- Unikaj N+1, czyli zapytań wykonywanych w pętli.
- Stosuj POST-Redirect-GET po udanym zapisie.
- Waliduj dane i sprawdzaj ModelState.IsValid.
- Paginuj duże listy.
- Operacje destrukcyjne wykonuj przez POST, nigdy przez GET.
- Nie ujawniaj connection stringów ani szczegółów wyjątków.
- Używaj logowania i przyjaznych stron błędów.
- Testuj PageModel oraz projekcje zapytań.

## 9. Docker: importer jako job

Compose uruchamia:

    mariadb + redis
          -> healthcheck
       import-job
          -> service_completed_successfully
       dashboard

import-job używa Dockerfile workera i uruchamia:

    dotnet PoliticalPaths.ImportWorker.dll full-sync

Komenda full-sync wykonuje kolejno:

1. db migrate — migracje EF Core,
2. sync — import z source-data/inbox,
3. extend — rozszerzenie danych przez API Sejmu.

Kontener ma restart: no, ponieważ jest jobem jednorazowym. Dashboard ma:

    depends_on:
      import-job:
        condition: service_completed_successfully

Nie wystartuje, jeśli import zakończy się błędem.

Importer czeka na zdrową MariaDB i Redis przez condition: service_healthy. W Dockerze localhost oznacza bieżący kontener, dlatego connection stringi używają nazw usług mariadb i redis.

Zmienne .NET w Compose mają podwójne podkreślenia:

    ConnectionStrings__MariaDb: "Server=mariadb;..."
    ConnectionStrings__Redis: "redis:6379,password=..."

Pliki wejściowe są montowane do /app/source-data jako read-only.

Uruchomienie:

    copy .env-example .env
    docker compose up --build

Dashboard działa pod http://localhost:8080.

Ponowienie joba i logi:

    docker compose run --rm import-job full-sync
    docker compose logs -f import-job
    docker compose logs -f dashboard

## 10. Typowe problemy

Jeżeli dashboard nie startuje, sprawdź docker compose ps oraz docker compose logs import-job. Warunek service_completed_successfully blokuje dashboard po błędzie importu.

Jeżeli brakuje plików, sprawdź source-data/inbox oraz montowanie do /app/source-data. Jeżeli baza jest niedostępna, sprawdź .env i czy hostem jest mariadb, a nie localhost.


## 11. Filtry Razor Pages

Filtry pozwalają wykonać kod przed lub po handlerze bez duplikowania go w wielu PageModelach. Najczęściej używa się ich do logowania, pomiaru czasu, sprawdzania uprawnień i obsługi wyjątków.

    public sealed class TimingPageFilter(ILogger<TimingPageFilter> logger)
        : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(
            PageHandlerExecutingContext context,
            PageHandlerExecutionDelegate next)
        {
            var start = Stopwatch.GetTimestamp();
            var executed = await next();
            var elapsed = Stopwatch.GetElapsedTime(start);
            logger.LogInformation("Page {Page} took {Elapsed} ms",
                context.ActionDescriptor.ViewEnginePath,
                elapsed.TotalMilliseconds);
        }

        public Task OnPageHandlerSelectionAsync(
            PageHandlerSelectedContext context) => Task.CompletedTask;
    }

Można zarejestrować filtr globalnie przez options.Conventions.ConfigureFilter<TimingPageFilter>() albo lokalnie dla konkretnej strony.

## 12. Konwencje i routing

Konwencje pozwalają zmieniać zachowanie całych folderów:

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/Admin");
    });

Parametry można ograniczać constraintami w @page, np. {id:guid}, {year:int} albo {slug:minlength(3)}. Do generowania adresów używaj asp-page i asp-route-*, a nie ręcznego składania URL.

## 13. Autoryzacja i bezpieczeństwo

Autoryzację można umieścić na PageModelu:

    [Authorize(Policy = "CanEditImports")]
    public sealed class EditModel : PageModel { }

Politykę rejestruje się w AddAuthorization. Zawsze sprawdzaj, czy zalogowany użytkownik ma prawo do rekordu wskazanego przez id z URL.

Zmiany danych wykonuj przez POST. Nie wyłączaj antiforgery bez konkretnego powodu. Operacje destrukcyjne nigdy nie powinny być wykonywane przez GET.

## 14. Zaawansowany model binding

Do formularzy używaj osobnych modeli wejściowych, nie encji EF:

    public sealed class ImportFilter
    {
        [StringLength(100)]
        public string? Pipeline { get; init; }

        [Range(1, 500)]
        public int PageSize { get; init; } = 50;
    }

    [BindProperty(SupportsGet = true)]
    public ImportFilter Filter { get; set; } = new();

Atrybut Bind może ograniczyć dozwolone pola i zapobiec overposting:

    [Bind("Name,Description")]
    public EditModel Input { get; set; } = new();

Dla niestandardowych typów można dodać IModelBinder lub IModelBinderProvider.

## 15. Walidacja i ModelState

ModelState zawiera dane po bindingu oraz błędy. Po zwróceniu Page() przy błędzie trzeba ponownie załadować listy wyboru i słowniki potrzebne do renderowania formularza.

    if (!ModelState.IsValid)
    {
        await LoadListsAsync(ct);
        return Page();
    }

Błędy biznesowe można dodać ręcznie przez ModelState.AddModelError. Walidacja zależna od bazy powinna być w serwisie aplikacyjnym, a nie tylko w atrybutach.

## 16. DI i lifetime DbContext

AddDbContext rejestruje DbContext jako Scoped. Jeden request powinien używać jednego kontekstu. Nie uruchamiaj równoległych zapytań na tym samym DbContext przez Task.WhenAll. W scenariuszu wymagającym równoległości użyj osobnych scope lub IDbContextFactory.

Nie twórz ręcznie AppDbContext ani ServiceProvider wewnątrz handlera.

## 17. Wydajność i cache

Dla stron read-only stosuj AsNoTracking, projekcję Select, paginację, filtrowanie w bazie i indeksy pod sortowania. Unikaj Include dla dużych grafów, jeśli wystarczy DTO.

ResponseCache może cache'ować publiczne, stabilne strony:

    [ResponseCache(Duration = 60,
        Location = ResponseCacheLocation.Any)]
    public sealed class IndexModel : PageModel { }

IMemoryCache lub cache rozproszony może przechowywać drogie agregacje. Po imporcie danych trzeba mieć strategię unieważniania cache.

## 18. Partial views i komponenty

Powtarzalny markup wydziel do Pages/Shared/_StatusBadge.cshtml:

    <partial name="_StatusBadge" model="Model.Status" />

Partial view nie powinien odpytwać bazy. Jeżeli fragment ma własną logikę, rozważ View Component albo osobny serwis. Wspólne usingi i Tag Helpers przechowuj w _ViewImports.cshtml, a layout w _ViewStart.cshtml.

## 19. TempData i POST-Redirect-GET

TempData służy do krótkich komunikatów pomiędzy requestami:

    TempData["Success"] = "Import zapisany.";
    return RedirectToPage();

Nie przechowuj w TempData dużych obiektów ani danych trwałych. Po udanym POST stosuj redirect, aby odświeżenie strony nie wysłało formularza ponownie.

## 20. Błędy i obserwowalność

W produkcji używaj app.UseExceptionHandler("/Error") i nie pokazuj stack trace użytkownikowi. Loguj wyjątki z identyfikatorem rekordu i kontekstem. EnableSensitiveDataLogging jest przydatne lokalnie, ale w produkcji może ujawnić dane i parametry zapytań.

Warto mierzyć czas handlerów, zapytań EF Core i jobów importu oraz przekazywać correlation/request ID.

## 21. Testowanie

PageModel można testować bez uruchamiania serwera. Serwis zapytań testuj osobno z bazą relacyjną lub Testcontainers. Provider InMemory nie odwzorowuje SQL i nie wykryje wielu problemów z tłumaczeniem LINQ.

    [Fact]
    public async Task MissingPoliticianReturnsNotFound()
    {
        var model = new DetailsModel(fakeQueries);
        var result = await model.OnGetAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

## 22. Background jobs a request HTTP

Razor Pages obsługuje request użytkownika. Import jest osobnym procesem, ponieważ może trwać długo i nie powinien blokować HTTP.

import-job czeka na healthchecki, wykonuje migracje, import i API, a potem kończy się kodem procesu. Dashboard zależy od jego sukcesu przez service_completed_successfully. To orkiestracja kontenerów, a nie komunikacja pomiędzy stronami.

W produkcji długie importy powinny mieć retry, timeout, idempotencję, blokadę równoległych uruchomień, historię wykonań i możliwość wznowienia.


## 23. Dlaczego worker w Dockerze widzi pliki

Wcześniejsza wersja importu miała ścieżkę wpisaną na stałe:

    C:\Users\matja\source\repos\PoliticalPaths\source-data\inbox

Była to ścieżka istniejąca tylko na komputerze deweloperskim. W kontenerze Linux
taki katalog nie istnieje, dlatego walidacja plików kończyła się błędem.

Obecnie worker korzysta z wartości przekazanej przez ImportSyncOptions:

    options.InboxRoot

Program workera wylicza tę wartość z konfiguracji:

    Import__InboxPath: "source-data/inbox"

W kontenerze katalogiem roboczym jest /app, dlatego pełna ścieżka staje się:

    /app/source-data/inbox

## 24. Jak działa volume mount

W docker-compose.yml znajduje się:

    volumes:
      - ./source-data:/app/source-data:ro

Lewa strona to katalog na hoście, czyli:

    ./source-data

Prawa strona to katalog wewnątrz kontenera:

    /app/source-data

Plik znajdujący się lokalnie jako:

    source-data/inbox/sejm.xlsx

jest więc widoczny w workerze jako:

    /app/source-data/inbox/sejm.xlsx

Flaga ro oznacza read-only. Importer może czytać pliki, ale nie może ich
nadpisać ani usunąć.

Można sprawdzić zawartość zamontowanego katalogu:

    docker compose run --rm --entrypoint ls import-job -la /app/source-data/inbox

Jeśli lista jest pusta, problem dotyczy katalogu na hoście albo ścieżki uruchomienia
docker compose, a nie kodu importera.

## 25. Dlaczego import działa jako job

import-job jest zwykłym procesem konsolowym .NET, a nie serwerem HTTP. Jego
zadaniem jest wykonać pracę i zakończyć się kodem procesu.

W Dockerfile workera entrypoint to:

    dotnet PoliticalPaths.ImportWorker.dll

Compose dodaje argument:

    command: ["full-sync"]

Po połączeniu powstaje:

    dotnet PoliticalPaths.ImportWorker.dll full-sync

Program rozpoznaje argument full-sync i wykonuje:

1. migracje bazy,
2. import plików,
3. rozszerzenie danych przez API Sejmu.

Po zakończeniu:

- kod 0 oznacza sukces,
- kod różny od zera oznacza błąd,
- kontener przechodzi w stan Exited,
- nie jest restartowany, ponieważ ma restart: "no".

Kontener nie musi działać cały czas. Jego wynikiem jest zmiana danych w MariaDB
oraz logi zapisane w zamontowanym katalogu logs/imports.

## 26. Jak Compose uruchamia dashboard po jobie

W Compose dashboard ma zależność:

    dashboard:
      depends_on:
        import-job:
          condition: service_completed_successfully

Compose wykonuje następujące kroki:

1. uruchamia MariaDB,
2. czeka na jej healthcheck,
3. uruchamia Redis,
4. czeka na jego healthcheck,
5. uruchamia import-job,
6. czeka, aż import-job zakończy się kodem 0,
7. dopiero wtedy uruchamia dashboard.

Jeśli import-job zakończy się błędem, krok 6 nie zostanie spełniony i dashboard
nie wystartuje. Jest to celowe: użytkownik nie powinien oglądać dashboardu,
gdy dane nie zostały poprawnie przygotowane.

Stan kontenerów można sprawdzić:

    docker compose ps

Logi importu:

    docker compose logs -f import-job

Logi dashboardu:

    docker compose logs -f dashboard

## 27. Ponowne wykonanie joba

Po zmianie plików wejściowych można wykonać tylko job:

    docker compose run --rm import-job full-sync

Opcja --rm usuwa tymczasowy kontener po zakończeniu, ale dane pozostają
w MariaDB, ponieważ baza korzysta z named volume.

Pełne przebudowanie obrazów wykonuje się przez:

    docker compose build --no-cache import-job dashboard

a pełny start przez:

    docker compose up

Import jest idempotentny względem checksum pliku. Plik już zapisany w bazie
zostanie pominięty, chyba że użyta zostanie opcja --force.

