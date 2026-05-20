# ADR-005: Start wyborczy jako Candidacy

## Decision

Każdy start w wyborach to osobna encja `Candidacy` z `SourceFingerprint`. Nie aktualizujemy „bieżącego stanu” polityka zamiast historii wyborów.

## Consequences

Analityka ścieżek wymaga joinów po `PoliticianId`; za to reimport nie niszczy historii.
