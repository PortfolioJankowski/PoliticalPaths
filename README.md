# PoliticalPaths

System do analizy ścieżek karier politycznych w Polsce (dane wyborcze, ETL z Excela, model relacyjny).

## Dokumentacja

| Dokument | Dla kogo |
|----------|----------|
| **[docs/DEVELOPER-GUIDE.md](docs/DEVELOPER-GUIDE.md)** | **Start tutaj** — F5, inbox, ImportBatch, flow, gdzie transformery |
| [docs/README.md](docs/README.md) | Architektura ETL |
| [docs/implementation-plan.md](docs/implementation-plan.md) | Plan iteracji |
| [docs/DATABASE-SCHEMA.md](docs/DATABASE-SCHEMA.md) | Tabele po `db migrate` |

## Szybki start (developer)

### 1. Baza (raz)

```bash
docker compose up -d
```

W Visual Studio: profil **„DB migrate”** albo:

```bash
dotnet run --project src/PoliticalPaths.ImportWorker -- db migrate
```

### 2. Import (codziennie)

1. Ustaw startup project: **`PoliticalPaths.ImportWorker`**
2. Profil: **„Sync pipelines (F5)”**
3. Wrzuć pliki `.xlsx` do **`source-data/inbox/{pipeline-key}/`** (np. `test-sample/` — pusty → automatyczny seed)
4. **F5**

Nie podajesz ścieżek w argumentach — aplikacja skanuje inbox.

Opcjonalny sidecar: `moj-plik.import.json` obok Excela (patrz [docs/DEVELOPER-GUIDE.md](docs/DEVELOPER-GUIDE.md)).

### ImportBatch / flow w skrócie

- **ImportBatch** = **jeden pipeline (transformer)** — nie jedno przypadkowe F5 ([ADR-013](docs/adr/013-batch-per-pipeline-sync.md))
- **Sync (F5)** = dla każdego pipeline: co już w bazie (SHA) → skip; nowe → full pipeline (RAW + transform)
- **ImportFile** = jeden Excel w batchu
- **ImportJob** / Hangfire — **nie na dev** (opcjonalnie kiedyś pod cron)

Szczegóły: **[docs/DEVELOPER-GUIDE.md](docs/DEVELOPER-GUIDE.md)**.

## Struktura `src/`

| Projekt | Rola |
|---------|------|
| `PoliticalPaths.ImportWorker` | **Entry point** — F5, `dev` |
| `PoliticalPaths.Application` | `IImportSyncService`, MediatR (RAW), inbox |
| `PoliticalPaths.Domain` | encje importu + domena wyborcza (KW) |
| `PoliticalPaths.Infrastructure` | EF Core, MariaDB |
| `PoliticalPaths.Importers.Raw` | Excel → `ImportRow`, rejestr pipeline |
| `PoliticalPaths.Importers.Transform` | rejestr transformerów (Faza 2) |
| `PoliticalPaths.Shared` | ścieżki repo, hash |

## Dane

| Folder | Rola |
|--------|------|
| `source-data/inbox/{pipeline-key}/` | Dev — jeden podfolder = jeden pipeline / batch |
| `source-data/{rok}/...` | Archiwum immutable (docelowy SOT) |

## Stan

**Faza 0 + 1** — sync per pipeline, pełny schemat DB. **Demo:** `test-sample` (prosty) + **`sejm-demo-2023`** (pełna domena w akcji) — [DEVELOPER-GUIDE](docs/DEVELOPER-GUIDE.md).
