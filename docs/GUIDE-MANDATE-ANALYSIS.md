# Analiza Kariery Politycznej - PoliticalPaths (SQL)

Poniższe zapytania SQL pozwalają prześledzić historię polityka, łącząc fakty wyborcze, sprawowane mandaty oraz zdarzenia nadzwyczajne.

## 1. Pełna oś czasu polityka (Timeline)
To zapytanie pokazuje chronologiczną listę wszystkich mandatów polityka wraz z ich aktualnym statusem.

```sql
SELECT 
    p.Nazwisko, 
    p.Imie, 
    m.DataOd, 
    m.DataDo, 
    m.Status, -- 1: Aktywny, 2: Wygasniety, 3: Zakonczony
    rw.Nazwa AS Organ,
    w.DataWyborow,
    (SELECT COUNT(*) FROM ZdarzeniaMandatowe zm WHERE zm.MandatId = m.Id) as LiczbaZdarzen
FROM Mandaty m
JOIN Politycy p ON m.PolitykId = p.Id
JOIN Wybory w ON m.WyboryId = w.Id
JOIN RodzajeWyborow rw ON w.RodzajWyborowId = rw.Id
WHERE p.Nazwisko = 'Nazwisko' -- Wpisz nazwisko
ORDER BY m.DataOd ASC;
```

## 1a. "Ultimate Career Path" (Złożona Oś Czasu)
To zapytanie generuje jedną, czytelną listę wszystkich zdarzeń z życia politycznego danej osoby, łącząc informacje o tym, do jakiego organu kandydował, kiedy objął mandat i kiedy go zakończył.

```sql
SELECT 
    zm.DataZdarzenia,
    CASE zm.Typ 
        WHEN 1 THEN 'WYBÓR (Wynik)'
        WHEN 2 THEN 'ŚLUBOWANIE (Aktywacja)'
        WHEN 3 THEN 'WSTĄPIENIE (Sukcesja)'
        WHEN 4 THEN 'WYGAŚNIĘCIE (Nadzwyczajne)'
        WHEN 5 THEN 'ZRZECZENIE SIĘ'
        WHEN 6 THEN 'OBJĘCIE INNEJ FUNKCJI'
        WHEN 7 THEN 'KONIEC KADENCJI'
        ELSE 'Inne'
    END AS TypZdarzenia,
    rw.Nazwa AS Organ,
    zm.Opis,
    m.Status AS StatusMandatuWChwiliObecnej
FROM ZdarzeniaMandatowe zm
JOIN Mandaty m ON zm.MandatId = m.Id
JOIN Wybory w ON m.WyboryId = w.Id
JOIN RodzajeWyborow rw ON w.RodzajWyborowId = rw.Id
JOIN Politycy p ON m.PolitykId = p.Id
WHERE p.Nazwisko = 'Nazwisko' AND p.Imie = 'Imię'
ORDER BY zm.DataZdarzenia ASC, zm.Typ ASC;
```

## 2. Szczegółowa historia konkretnego mandatu (Zdarzenia)
Jeśli polityk ma mandat, który wygasł (np. rezygnacja), to zapytanie pokaże dokładnie dlaczego i kiedy.

```sql
SELECT 
    zm.DataZdarzenia,
    zm.Typ, -- 2: Objecie, 5: Zrzeczenie, 6: InnaFunkcja
    zm.Opis,
    zm.DokumentReferencyjny
FROM ZdarzeniaMandatowe zm
JOIN Mandaty m ON zm.MandatId = m.Id
JOIN Politycy p ON m.PolitykId = p.Id
WHERE p.Nazwisko = 'Nazwisko' AND m.DataOd = '2019-10-13'
ORDER BY zm.DataZdarzenia ASC;
```

## 3. Sprawdzenie sukcesji (Kto wszedł za kogo)
Pokaż mandaty, które nie wynikają bezpośrednio z wygranych wyborów (sukcesja).

```sql
SELECT 
    p.Nazwisko, 
    p.Imie, 
    m.DataOd, 
    rw.Nazwa AS Organ,
    s.Id as StartWyborczyId
FROM Mandaty m
JOIN Politycy p ON m.PolitykId = p.Id
JOIN StartyWyborcze s ON m.StartWyborczyId = s.Id
JOIN Wybory w ON m.WyboryId = w.Id
JOIN RodzajeWyborow rw ON w.RodzajWyborowId = rw.Id
WHERE m.TypObjecia = 1 -- 1: Sukcesja
ORDER BY m.DataOd DESC;
```

## 4. Statystyki: Ilu posłów nie ukończyło kadencji?
Zapytanie pomocne do analizy rotacji w parlamencie.

```sql
SELECT 
    rw.Nazwa, 
    w.DataWyborow,
    COUNT(m.Id) as LiczbaWygaslychMandatow
FROM Mandaty m
JOIN Wybory w ON m.WyboryId = w.Id
JOIN RodzajeWyborow rw ON w.RodzajWyborowId = rw.Id
WHERE m.Status = 2 -- Wygasniety
GROUP BY rw.Nazwa, w.DataWyborow;
```

---

## Strategia Importu Zdarzeń (MD)

Zgodnie z Twoim pomysłem, najlepszą ścieżką jest:
1. **Import Wyborów**: Standardowy proces Excel -> `MandateGeneratorService` (tworzy mandaty `Oczekujące`).
2. **Plik Zdarzeń**: Przechowuj zdarzenia w pliku (np. `zdarzenia_nadzwyczajne.json` lub `.csv`).
3. **Batch Update**: Po wszystkich importach uruchom skrypt/serwis, który:
   - Znajduje `MandatId` na podstawie `PolitykId` i `WyboryId`.
   - Wywołuje `MandateEventService.AddEventAsync` dla każdego wpisu.

Dzięki temu Twoja baza będzie zawsze spójna, nawet jeśli dane o rezygnacjach otrzymasz dużo później niż same wyniki wyborów.
