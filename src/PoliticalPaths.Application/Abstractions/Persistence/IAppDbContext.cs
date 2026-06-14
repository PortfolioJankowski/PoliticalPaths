using PoliticalPaths.Domain.Formacje;
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
    DbSet<Polityk> Politycy { get; }
    DbSet<Partia> Partie { get; }
    DbSet<PartiaCzlonkostwo> PartieCzlonkostwa { get; }
    DbSet<Wybory> Wybory { get; }
    DbSet<RodzajeWyborow> RodzajeWyborow { get; }
    DbSet<OkregWyborczy> OkregWyborczy { get; }
    DbSet<SzczegolyOkregu> SzczegolyOkregow { get; }
    DbSet<KomitetWyborczy> KomitetyWyborcze { get; }
    DbSet<ListaWyborcza> ListaWyborcza { get; }
    DbSet<StartWyborczy> StartyWyborcze { get; }
    DbSet<WynikiWyborow> WynikiWyborow { get; }
    DbSet<Kadencja> Kadencje { get; }
    DbSet<Mandat> Mandaty { get; }
    DbSet<ZdarzenieMandatowe> ZdarzeniaMandatowe { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
