# ADR-008: GraphQL po stabilizacji ETL

## Decision

GraphQL (Hot Chocolate) dopiero po stabilnym imporcie i modelu. Teraz: MediatR queries w Application, cienka warstwa API później.

## Consequences

Brak przedwczesnej złożoności w Api; queries analityczne projektujemy pod EF, nie pod kształt GraphQL.
