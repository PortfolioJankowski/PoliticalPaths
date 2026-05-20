# PoliticalPaths

System do analizy ścieżek karier politycznych w Polsce (dane wyborcze, ETL z Excela, model relacyjny).

## Dokumentacja

**[docs/README.md](docs/README.md)** — architektura ETL, transformery, plan implementacji.

**Model domenowy (okręgi, TERYT, listy, wyniki):** **[docs/architecture/05-domain-model.md](docs/architecture/05-domain-model.md)**

Szybki start planu: **[docs/implementation-plan.md](docs/implementation-plan.md)**.

### Model danych w skrócie

- **Okręgi** — osobno dla Sejmu, Senatu i sejmików; powiązane z konkretnymi **wyborami** (`Election`), nie jedna globalna tabela numerów.
- **TERYT** — jednostki terytorialne (województwo / powiat / miasto); okręg **obejmuje** obszar (relacja M:N), kandydat startuje w **okręgu wyborczym**.
- **Statystyki okręgu** (mieszkańcy, uprawnieni, …) — **`ElectoralDistrictSnapshot`** per wybory; wartości z 2019 i 2023 to osobne rekordy.
- **Listy** — w obrębie okręgu (Sejm, sejmik); **Senat bez list** — głos na kandydata w jednomandatowym okręgu.
- **Komitety wyborcze** — oddzielnie od partii (`ElectoralCommittee`).
- **Start** = `Candidacy` (profil zależny od typu wyborów: z listą lub tylko z komitetem).
- **Zgodność z KW** — [docs/architecture/09-domain-model-validation-kw.md](docs/architecture/09-domain-model-validation-kw.md).
- **Mandat w kadencji** (wygaśnięcie, następcy z listy) — [docs/architecture/10-mandate-lifecycle.md](docs/architecture/10-mandate-lifecycle.md); wynik `Elected` ≠ kto był posłem w danym roku.
- **Wyniki** — zawsze w kontekście roku/wyborów; inne lata = inne wiersze w tabelach wyników.

## Dane źródłowe

Katalog **[source-data/](source-data/)** — immutable pliki Excel (konwencje nazewnictwa w `source-data/README.md`).

## Stan projektu

Szkielet aplikacji konsolowej; implementacja według faz w planie. Następna iteracja: **Faza 0 + 1** (solution + warstwa techniczna importu RAW).
