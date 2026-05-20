# Biblioteki i GraphQL (przyszłość)

## NuGet — rekomendacja

| Obszar | Pakiet |
|--------|--------|
| EF + MariaDB | `Pomelo.EntityFrameworkCore.MySql` |
| MediatR | `MediatR`, `MediatR.Extensions.Microsoft.DependencyInjection` |
| Walidacja | `FluentValidation.DependencyInjectionExtensions` |
| Excel | **ClosedXML** (MIT) |
| Skan DI | `Scrutor` (rejestr transformerów) |
| Logowanie | `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Serilog.Formatting.Compact` |
| Joby | `Hangfire.AspNetCore`, `Hangfire.Storage.MySql` |
| Odporność | `Polly` |
| Testy | `xunit`, `FluentAssertions`, `Testcontainers.MySql` |
| GraphQL (później) | `HotChocolate.AspNetCore`, `HotChocolate.Data.EntityFramework` |

## Excel: ClosedXML vs EPPlus

| | ClosedXML | EPPlus 5+ |
|---|-----------|-----------|
| Licencja | MIT | Noncommercial / komercyjna płatna |
| Duże pliki | Cały workbook w pamięci | Podobnie |
| API | Wygodne (LINQ) | Bardziej niskopoziomowe |

**Decyzja:** ClosedXML. Przy bardzo dużych plikach — batch insert + ewentualnie `ExcelDataReader` tylko do odczytu wierszy na etapie RAW.

## GraphQL — kiedy

| Faza | Działanie |
|------|-----------|
| Teraz | Nie dodawać — skupienie na ETL i jakości danych |
| Po stabilnym modelu | Hot Chocolate; resolvery delegują do MediatR queries |

### Przygotowanie już teraz

- Zapytania analityczne jako `IRequest<T>` w Application.
- Brak logiki w resolverach (cienka warstwa Api).

### Przykładowe query (docelowo)

```graphql
query PoliticianCareer($id: UUID!) {
  politician(id: $id) {
    canonicalFullName
    aliases { fullName type validFrom validTo }
    candidacies(order: { election: { year: DESC } }) {
      election { year type }
      electoralList { name party { name } }
      district { number name }
      voteResult { votesReceived percent elected }
    }
    partyAffiliations { party { name } validFrom validTo }
    clubMemberships { club { name } validFrom validTo }
  }
}
```

Patrz ADR-008 w [adr/README.md](../adr/README.md).
