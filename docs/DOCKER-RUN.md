# Uruchomienie całego systemu w Docker Compose

1. Skopiuj `.env-example` do `.env` i ustaw hasła.
2. Umieść pliki wejściowe w `source-data/inbox`.
3. Uruchom:

```bash
docker compose up --build
```

Compose uruchamia MariaDB i Redis, następnie kontener `import-job` wykonuje
migrację bazy, import plików oraz rozszerzenie danych przez API Sejmu. Dashboard
zostaje uruchomiony dopiero po pomyślnym zakończeniu joba i jest dostępny pod
`http://localhost:8080`.

Job jest jednorazowy (`restart: no`). Po zmianie danych wejściowych można go
uruchomić ponownie bez przebudowy obrazów:

```bash
docker compose run --rm import-job full-sync
```

