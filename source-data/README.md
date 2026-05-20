# Source data — konwencje

Pliki tutaj są **nietykalne** po dodaniu. Zobacz [docs/architecture/02-source-data.md](../docs/architecture/02-source-data.md).

## Struktura

```
{rok}/{typ-wyborow}/{organ}/{etap}/v{format-version}/{sha8}_{logical-name}_{yyyy-MM-dd}.xlsx
```

## Logical names (słownik)

Dodawaj wpisy przy każdym nowym typie pliku. Muszą odpowiadać atrybutowi `[ImportTransformer(LogicalNames = ...)]`.

| Logical name | Opis | Transformer (klasa) | Status |
|--------------|------|---------------------|--------|
| *(przykład)* `sejm-2023-listy-kandydatow` | Listy kandydatów Sejm 2023 | TBD | planowany |

## Manifest

Opcjonalnie: `manifest/{import-batch-id}.json` — lista plików w jednej dostawie.

## Git LFS

Pliki `.xlsx` większe niż ~5 MB — konfiguruj LFS przed pierwszym dużym commitem.
