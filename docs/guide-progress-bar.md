# Przewodnik: Konsolowy Progress Bar (Spectre.Console)

Ten dokument wyjaśnia, jak zaimplementowano wizualizację postępu importu w konsoli, od podstaw aż po zaawansowane wzorce .NET.

---

## 1. Wybór biblioteki: Spectre.Console
Zamiast ręcznie operować na kursorze konsoli (`Console.SetCursorPosition`), użyliśmy biblioteki **Spectre.Console**. Jest to standard w nowoczesnych aplikacjach CLI .NET, oferujący:
- Rich text (kolory, style).
- Dynamiczne komponenty (loading bary, tabele, spinnery).
- Pełną obsługę ANSI.

## 2. Architektura raportowania postępu
Implementacja opiera się na standardowym wzorcu .NET `IProgress<T>`.

### Poziom Beginner: Czym jest `IProgress<T>`?
To wbudowany w .NET interfejs, który służy do przekazywania informacji o postępie z zadań asynchronicznych do wątku UI (lub konsoli). Dzięki temu logika biznesowa nie musi wiedzieć nic o "pasku postępu" – ona tylko mówi: "zrobiłem 10 z 100".

### Poziom Intermediate: Struktura danych
Stworzyliśmy rekordy, które niosą kontekst:
- `TransformationProgress`: (Current, Total) – używany wewnątrz transformera.
- `ImportProgressInfo`: (PipelineKey, FileName, Current, Total, IsCompleted) – używany w orkiestratorze, aby CLI wiedziało, który pasek zaktualizować.

### Poziom Senior: Orkiestracja i mapowanie
W `ImportSyncService.cs` stosujemy mapowanie progresu:
```csharp
var innerProgress = progress != null 
    ? new Progress<TransformationProgress>(p => progress.Report(new ImportProgressInfo(..., p.Current, p.Total)))
    : null;
```
Dlaczego? Bo `ExcelFileTransformerBase` operuje na liczbach (Stage 2), a CLI potrzebuje wiedzieć, który to plik i pipeline (User Experience).

---

## 3. Implementacja w CLI (`Program.cs`)
Użyliśmy `AnsiConsole.Progress()`, który tworzy interaktywny kontekst.

```csharp
await AnsiConsole.Progress()
    .Columns(new ProgressColumn[] { ... })
    .StartAsync(async ctx => { ... });
```

**Kluczowe mechanizmy:**
- **Dynamiczne zadania**: Ponieważ nie wiemy z góry, ile będzie plików, używamy `Dictionary<string, ProgressTask>`. Zadania są tworzone w locie (`ctx.AddTask`), gdy tylko wpłynie pierwszy raport o postępie.
- **Auto-skalowanie**: Jeśli podczas "Lazy Import" odkryjemy więcej wierszy, `task.MaxValue` jest aktualizowane dynamicznie.

---

## 4. Wskazówki dla Seniora
1. **Thread Safety**: `Progress<T>` domyślnie przechwytuje kontekst synchronizacji. W aplikacjach konsolowych wywołuje callback na wątku z puli, co Spectre.Console obsługuje bezpiecznie (internal locking).
2. **Separation of Concerns**: Logika transformacji nie zależy od Spectre.Console. Zależy tylko od abstrakcji `IProgress<T>`, co pozwala na łatwą zmianę CLI na np. SignalR w przyszłości.
3. **Overhead**: Raportowanie postępu po każdym wierszu (`progress?.Report`) przy 100k wierszy może obciążyć procesor. W bardzo dużych importach warto raportować co N wierszy lub co X milisekund.
