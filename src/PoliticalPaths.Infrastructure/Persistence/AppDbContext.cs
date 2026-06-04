using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Geografia;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    // ETL
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportFile> ImportFiles => Set<ImportFile>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    public DbSet<TransformationError> TransformationErrors => Set<TransformationError>();

    // Domena
    public DbSet<Teryt> Teryt => Set<Teryt>();
    public DbSet<Politycy> Politycy => Set<Politycy>();
    public DbSet<Formacje> Formacje => Set<Formacje>();
    public DbSet<Kluby> Kluby => Set<Kluby>();
    public DbSet<KlubyCzlonkowstwo> KlubyCzlonkowstwo => Set<KlubyCzlonkowstwo>();
    public DbSet<MapaWyborow> MapaWyborow => Set<MapaWyborow>();
    public DbSet<SlownikWyborow> SlownikWyborow => Set<SlownikWyborow>();
    public DbSet<OkregWyborczy> OkregWyborczy => Set<OkregWyborczy>();
    public DbSet<MapaOkregow> MapaOkregow => Set<MapaOkregow>();
    public DbSet<LudnoscOkregow> LudnoscOkregow => Set<LudnoscOkregow>();
    public DbSet<KomitetyWyborcze> KomitetyWyborcze => Set<KomitetyWyborcze>();
    public DbSet<ListaWyborcza> ListaWyborcza => Set<ListaWyborcza>();
    public DbSet<StartyWyborcze> StartyWyborcze => Set<StartyWyborcze>();
    public DbSet<WynikiWyborow> WynikiWyborow => Set<WynikiWyborow>();
    public DbSet<Kadencja> Kadencje => Set<Kadencja>();
    public DbSet<Mandat> Mandaty => Set<Mandat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Geografia
        modelBuilder.Entity<Teryt>(b =>
        {
            b.HasKey(x => x.KodTeryt);
            b.Property(x => x.KodTeryt).HasMaxLength(10);
            b.Property(x => x.Nazwa).HasMaxLength(200);
        });

        // Politycy
        modelBuilder.Entity<Politycy>(b =>
        {
            b.Property(x => x.Imie).HasMaxLength(100);
            b.Property(x => x.Nazwisko).HasMaxLength(100);
        });

        // Wybory
        modelBuilder.Entity<MapaOkregow>(b =>
        {
            b.HasKey(x => new { x.KodTeryt, x.OkregWyborczyId });
        });

        modelBuilder.Entity<LudnoscOkregow>(b =>
        {
            b.HasKey(x => new { x.OkregId, x.RokWyborow });
        });

        modelBuilder.Entity<WynikiWyborow>(b =>
        {
            b.HasKey(x => x.StartId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
