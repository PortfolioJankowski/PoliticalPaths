# Inbox (dev)

Jeden **podfolder = jeden pipeline** (jeden transformer = jeden `ImportBatch` w bazie).

```
inbox/
  test-sample/           ← prosty wzór (import + błędy)
  sejm-demo-2023/        ← pełna domena (5 arkuszy, mandaty, kluby)
  sejm-2023-listy/       ← docelowy produkcyjny format
    ...
```

**F5 / `sync`:** dla każdego zarejestrowanego pipeline skanuje folder → porównuje SHA z plikami w batchu → importuje tylko nowe (RAW + transform).

Pusty `test-sample/` → automatyczny seed `test-sample.xlsx`.

Archiwum immutable → [../README.md](../README.md).
