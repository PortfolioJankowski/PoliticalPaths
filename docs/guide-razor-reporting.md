# Przewodnik: Raportowanie HTML przez RazorLight

Ten dokument opisuje, jak system generuje profesjonalne raporty HTML po zakończeniu synchronizacji danych.

---

## 1. Silnik: RazorLight
Użyliśmy biblioteki **RazorLight**, która pozwala na renderowanie plików `.cshtml` (Razor) poza środowiskiem ASP.NET Core (w aplikacji konsolowej).

## 2. Konfiguracja projektu (.csproj)
Aby szablony były dostępne w skompilowanej aplikacji konsolowej bez konieczności kopiowania plików fizycznych, osadziliśmy je jako zasoby:

```xml
<ItemGroup>
  <EmbeddedResource Include="Imports\Templates\*.cshtml" />
</ItemGroup>
```

**Zaleta Seniora:** Pliki te stają się częścią DLL. Nie musisz martwić się o `Directory.GetCurrentDirectory()` czy brakujące pliki na serwerze produkcyjnym.

---

## 3. Implementacja `ImportReportService`

### Inicjalizacja silnika
```csharp
_engine = new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(ImportReportService))
    .UseMemoryCachingProvider()
    .Build();
```
- `UseEmbeddedResourcesProject`: Mówi RazorLight, aby szukał widoków wewnątrz assembly.
- `UseMemoryCachingProvider`: Kompiluje widok tylko raz i trzyma w pamięci. To krytyczne dla wydajności.

### Renderowanie
```csharp
var html = await _engine.CompileRenderAsync("Imports.Templates.ImportReport", result);
```
Ścieżka do widoku to kropkowy zapis przestrzeni nazw wewnątrz zasobów osadzonych.

---

## 4. Szablon i CSS
Plik `ImportReport.cshtml` to standardowy dokument HTML5 z osadzonym CSSem.

### Dlaczego Inline CSS?
W raportach generowanych do plików (offline) nie możemy linkować do zewnętrznych arkuszy stylów (chyba że przez CDN, ale to wymaga internetu). Użyliśmy:
- **Flexbox**: Do kart podsumowania (`.summary-cards`).
- **Google Fonts (Fallback)**: Bezpieczne fonty systemowe dla szybkości.
- **Semantic UI logic**: Klasy typu `.status.success` do kolorowania wyników.

---

## 5. Senior Dev: Co musisz wiedzieć?
1. **Strong Typing**: Szablon zaczyna się od `@model ImportSyncResult`. Dzięki temu mamy pełne wsparcie IntelliSense i type-safety przy renderowaniu.
2. **XSS Protection**: Razor domyślnie koduje wszystkie wartości (`@Model.Value`). Jeśli chcesz wyrenderować czysty HTML (np. sformatowany log), musisz użyć `@Html.Raw()`.
3. **Resource Leakage**: RazorLight tworzy tymczasowe klasy w pamięci dla każdego szablonu. Dzięki `UseMemoryCachingProvider` unikamy wycieku pamięci przy wielokrotnym generowaniu raportu.
