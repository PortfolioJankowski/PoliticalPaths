# Architecture Decision Records (ADR)

Krótkie uzasadnione decyzje — żeby nie rozstrzygać ponownie za rok.

| ID | Tytuł | Status |
|----|-------|--------|
| [001](001-immutable-source-data.md) | Immutable `source-data` + SHA w nazwie | Accepted |
| [002](002-two-stage-etl.md) | ETL dwuetapowy RAW → Transform | Accepted |
| [003](003-import-technical-entities.md) | Encje ImportBatch/File/Row/TransformationError | Accepted |
| [004](004-manual-transformers-with-attributes.md) | Ręczne transformery + atrybut `LogicalNames` | Accepted |
| [005](005-candidacy-as-electoral-fact.md) | Start wyborczy = `Candidacy`, nie mutacja polityka | Accepted |
| [006](006-closedxml.md) | ClosedXML (MIT) na etapie Excel | Accepted |
| [007](007-identity-resolution-strategy.md) | Identity resolution: score + manual override | Accepted |
| [008](008-graphql-later.md) | GraphQL (Hot Chocolate) po stabilizacji ETL | Accepted |
| [009](009-reimport-history.md) | Reimport = nowy batch, historia zachowana | Accepted |
| [010](010-electoral-district-per-election.md) | Okręgi per Election + chamber, snapshoty, listy w okręgu | Accepted |
| [011](011-senate-no-electoral-lists.md) | Senat: brak `ElectoralList`, profil `SenateMajoritarian` | Accepted |
| [012](012-mandate-lifecycle-separate-from-election-results.md) | `Mandate` / kadencja oddzielnie od `Elected` | Accepted |

## Format nowego ADR

```
docs/adr/NNN-krotki-tytul.md
```

Sekcje: Context, Decision, Consequences.
