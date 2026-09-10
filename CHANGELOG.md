# Changelog

Najważniejsze zmiany w Political Paths. Wpisy opisują funkcje istotne dla użytkowników i utrzymania systemu, a nie każdy pojedynczy commit.

## Unreleased

### Security

- Dodano logowanie, role Admin i User oraz domyślną autoryzację całego Dashboardu.
- Dodano ochronę logowania i aplikacji przez warstwowy rate limiting z fallbackiem po adresie IP.

### Added

- Dodano panel administratora do tworzenia, blokowania i zarządzania rolami kont.
- Dodano widok changelogu i podsumowanie ostatnich zmian na dashboardzie.
- Dodano szczegółową dokumentację bazy danych, importu i usług uzupełniających.

## 2026-09-07

### Added

- Udostępniono Dashboard do przeglądania importów, wyborów i polityków.
- Rozbudowano dokumentację użytkową i techniczną projektu.

### Changed

- Zoptymalizowano zapytania warstwy danych wykorzystywane przez Dashboard.

## 2026-09-01

### Added

- Dodano obsługę danych wyborów do Sejmu z 2011 i 2015 roku.
- Dodano serwis przydziału mandatów metodą D’Hondta.

## 2026-08-23

### Added

- Dodano rozpoznawanie następców obejmujących mandat po jego wygaśnięciu.

## 2026-08-09

### Added

- Dodano pobieranie danych posłów z API Sejmu i uzupełnianie rekordów lokalnych.

### Changed

- Rozdzielono DTO, mapowania, klienta API i serwisy infrastrukturalne zgodnie z warstwami rozwiązania.
- Zapewniono idempotencję zdarzeń dotyczących mandatów.

## 2026-08-01

### Added

- Zaimportowano dane wyborów do Sejmu z 2019 roku.
- Dodano usługę rozpoznawania imion i nazwisk.

### Changed

- Przeniesiono rozwiązanie na platformę .NET 10.

## 2026-06-13

### Added

- Dodano konsolowy pasek postępu i czytelne podsumowanie importu.

### Fixed

- Ustabilizowano raportowanie oraz przetwarzanie błędów importu.

## 2026-06-07

### Added

- Dodano import wielu plików w ramach pipeline’u, raporty HTML i cache Redis.
- Ukończono pierwszy produkcyjny schemat bazy i mechanizm rozwiązywania encji.

### Changed

- Uporządkowano migracje i strukturę procesu ETL.

## 2026-05-31

### Changed

- Dostosowano model EF Core do ograniczeń MariaDB i uproszczono strukturę rozwiązania.

## 2026-05-20

### Added

- Utworzono projekt Political Paths, pierwszą dokumentację architektury i fundament modelu domenowego.
