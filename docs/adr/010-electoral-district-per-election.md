# ADR-010: Okręgi per wybory i chamber, snapshoty statystyk

## Context

Okręgi Sejmu, Senatu i sejmików różnią się numeracją i granicami. Liczba mieszkańców i uprawnionych zmienia się między wyborami. Wyniki i listy są specyficzne dla roku wyborów.

## Decision

1. `ElectoralDistrict` jest powiązany z `Election` i `ElectoralChamber` (Sejm | Senate | RegionalAssembly).
2. TERYT: `TerritorialUnit` + M:N `ElectoralDistrictTerritory`.
3. Statystyki okręgu: `ElectoralDistrictSnapshot` (nie kolumny na `ElectoralDistrict`).
4. `ElectoralList` wymaga `ElectoralDistrictId`.
5. `Candidacy` wymaga okręgu i listy; wyniki w osobnych encjach z `ElectionId`.

## Consequences

Więcej tabel i joinów, ale poprawna analiza historyczna i brak cichego nadpisywania danych z innych lat.
