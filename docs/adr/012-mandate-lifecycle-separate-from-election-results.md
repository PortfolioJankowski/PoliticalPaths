# ADR-012: Mandat (kadencja) oddzielnie od wyniku wyborów

## Context

Wyniki PKW (`Elected`, głosy) nie opisują dynamicznego składu Sejmu/Senatu/sejmiku w czasie: wygaśnięcie mandatu (art. 247, 383), obsadzenie z listy (art. 251), wybory uzupełniające (art. 283).

## Decision

1. Wprowadzić `LegislativeTerm`, `Mandate`, `MandateEvent`, `ElectionMandateAllocation`.
2. `CandidacyVoteResult.Elected` — tylko wynik wyborów; **źródło prawdy „kto był posłem”** = `Mandate` z `ValidFrom`/`ValidTo`.
3. Sukcesja: `PredecessorMandateId`, `MandateAcquisitionType` (lista / wybory uzupełniające).
4. Import mandatów — osobne źródła / ręcznie; nie wyprowadzać pełnej kadencji wyłącznie z Excela wyników.

## Consequences

Więcej tabel i pracy przy uzupełnianiu danych historycznych; za to poprawne ścieżki kariery i analiza składu w dowolnym punkcie czasu.
