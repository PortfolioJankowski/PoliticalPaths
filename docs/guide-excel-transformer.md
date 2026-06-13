# Przewodnik: ExcelFileTransformerBase i Lazy Raw Import

To serce mechanizmu ETL. Klasa ta łączy "surowy" świat Excela z modelem domenowym Entity Framework Core.

---

## 1. Wzorzec "Template Method"
`ExcelFileTransformerBase` to klasa abstrakcyjna. Definiuje ona *szkielet* algorytmu w metodzie `ProcessRowsAsync`, ale szczegóły przetwarzania wiersza zostawia podklasom (np. `SejmDemo2023Transformer`).

## 2. Mechanizm "Lazy Raw Import"
Rozwiązaliśmy problem "zimnego startu" (pustej bazy danych).

**Logika:**
1. Pobierz wszystkie istniejące `ImportRow` dla danego pliku do słownika (`rowsMap`).
2. Iteruj po wierszach fizycznego pliku Excel.
3. Jeśli wiersza nie ma w `rowsMap` -> Stwórz go natychmiast (`Db.ImportRows.Add`).
4. Przekaż wiersz (nowy lub istniejący) do logiki domenowej.

---

## 3. Wyjaśnienie: EF Core ValueComparer i Expressions
Przy zmianie `LogicalName` (string) na `LogicalNames` (tablica `string[]`), napotkaliśmy wyzwanie techniczne w `ImportFileConfiguration.cs`.

### Problem
EF Core nie wie, jak porównywać tablice. Domyślnie porównuje referencje. Jeśli pobierzesz tablicę z bazy i stworzysz nową z tymi samymi danymi, EF uzna, że nic się nie zmieniło.

### Rozwiązanie: `ValueComparer<string[]>`
Musieliśmy zdefiniować trzy funkcje (wyrażenia lambda):
1. **Equals**: `(a, b) => a.SequenceEqual(b)` – porównuje zawartość, nie referencję.
2. **HashCode**: `a => a.Aggregate(...)` – generuje stabilny hash na podstawie elementów.
3. **Snapshot**: `a => a.ToArray()` – tworzy głęboką kopię do śledzenia zmian (Change Tracking).

### Dlaczego Expressions (Drzewa Wyrażeń)?
EF Core konfiguracja wymaga, aby te lambdy były przekazywane jako `Expression<Func<...>>`.
**Ograniczenia Seniora:**
- Nie można używać bloków instrukcji `{ ... }`. Musi to być "single expression".
- Nie można używać operatora `?.` (null-propagation) w niektórych wersjach providerów, dlatego użyliśmy jawnych sprawdzeń `(a == null ? 0 : ...)`.

---

## 4. Generyczne Parsowanie (Enum jako Indexer)
Zaimplementowaliśmy metody `GetValue<TEnum>`, które rzutują enum na `int`.

```csharp
var index = Convert.ToInt32(column);
return row.Values[index];
```

**Dlaczego to jest "Senior-level"?**
- **Type Safety**: Dzięki constraintowi `where TEnum : struct, Enum`, kompilator pilnuje, abyś nie przekazał tam przypadkowego typu.
- **Performance**: Rzutowanie enuma na int jest operacją O(1) i nie powoduje boxingu w nowoczesnym .NET.
- **Maintainability**: Jeśli PKW zmieni nazwę kolumny, ale nie jej pozycję – Twój kod działa. Jeśli zmieni pozycję – zmieniasz tylko jedną liczbę w enumie, nie dotykając logiki biznesowej.
