using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Formacje;
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
    public DbSet<Politycy> Politycy => Set<Politycy>();
    public DbSet<Formacje> Formacje => Set<Formacje>();
    public DbSet<Kluby> Kluby => Set<Kluby>();
    public DbSet<KlubyCzlonkowstwo> KlubyCzlonkowstwo => Set<KlubyCzlonkowstwo>();
    public DbSet<Wybory> MapaWyborow => Set<Wybory>();
    public DbSet<RodzajeWyborow> SlownikWyborow => Set<RodzajeWyborow>();
    public DbSet<OkregWyborczy> OkregWyborczy => Set<OkregWyborczy>();
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

        // Politycy
        modelBuilder.Entity<Politycy>(b =>
        {
            b.Property(x => x.Imie).HasMaxLength(100);
            b.Property(x => x.Nazwisko).HasMaxLength(100);
            b.Property(x => x.MiejsceUrodzenia).HasMaxLength(200);
        });

        // Wybory
        modelBuilder.Entity<Wybory>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.DataWyborow).IsRequired();
            b.Property(x => x.Ordynacja).HasConversion<int>();
            b.Property(x => x.Tura).HasConversion<int>();
            b.Property(x => x.CzyPrzedterminowe).HasDefaultValue(false);
        });

        modelBuilder.Entity<RodzajeWyborow>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Nazwa).HasMaxLength(100).IsRequired();
            b.Property(x => x.Poziom).HasConversion<int>();
        });

        modelBuilder.Entity<OkregWyborczy>(b =>
        {
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<LudnoscOkregow>(b =>
        {
            b.HasKey(x => new { x.OkregId, x.RokWyborow });
        });

        modelBuilder.Entity<WynikiWyborow>(b =>
        {
            b.HasKey(x => x.StartId);
        });

        modelBuilder.Entity<StartyWyborcze>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Zawod).HasMaxLength(200);
            b.Property(x => x.Wyksztalcenie).HasMaxLength(200);
            b.Property(x => x.MiejsceZamieszkania).HasMaxLength(200);
        });

        base.OnModelCreating(modelBuilder);
    }
}
