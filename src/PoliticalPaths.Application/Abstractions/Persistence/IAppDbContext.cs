using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Geografia;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace PoliticalPaths.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    // ETL
    DbSet<ImportBatch> ImportBatches { get; }
    DbSet<ImportFile> ImportFiles { get; }
    DbSet<ImportRow> ImportRows { get; }
    DbSet<TransformationError> TransformationErrors { get; }

    // Domena
    DbSet<Teryt> Teryt { get; }
    DbSet<Politycy> Politycy { get; }
    DbSet<Formacje> Formacje { get; }
    DbSet<Kluby> Kluby { get; }
    DbSet<KlubyCzlonkowstwo> KlubyCzlonkowstwo { get; }
    DbSet<Wybory> MapaWyborow { get; }
    DbSet<SlownikWyborow> SlownikWyborow { get; }
    DbSet<OkregWyborczy> OkregWyborczy { get; }
    DbSet<MapaOkregow> MapaOkregow { get; }
    DbSet<LudnoscOkregow> LudnoscOkregow { get; }
    DbSet<KomitetyWyborcze> KomitetyWyborcze { get; }
    DbSet<ListaWyborcza> ListaWyborcza { get; }
    DbSet<StartyWyborcze> StartyWyborcze { get; }
    DbSet<WynikiWyborow> WynikiWyborow { get; }
    DbSet<Kadencja> Kadencje { get; }
    DbSet<Mandat> Mandaty { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
