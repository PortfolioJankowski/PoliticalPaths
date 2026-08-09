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
    public DbSet<Polityk> Politycy => Set<Polityk>();
    public DbSet<Partia> Formacje => Set<Partia>();
    public DbSet<Partia> Partie => Set<Partia>();
    public DbSet<PartiaCzlonkostwo> PartieCzlonkostwa => Set<PartiaCzlonkostwo>();
    public DbSet<Wybory> Wybory => Set<Wybory>();
    public DbSet<RodzajeWyborow> RodzajeWyborow => Set<RodzajeWyborow>();
    public DbSet<OkregWyborczy> OkregWyborczy => Set<OkregWyborczy>();
    public DbSet<SzczegolyOkregu> SzczegolyOkregow => Set<SzczegolyOkregu>();
    public DbSet<KomitetWyborczy> KomitetyWyborcze => Set<KomitetWyborczy>();
    public DbSet<ListaWyborcza> ListaWyborcza => Set<ListaWyborcza>();
    public DbSet<StartWyborczy> StartyWyborcze => Set<StartWyborczy>();
    public DbSet<WynikiWyborow> WynikiWyborow => Set<WynikiWyborow>();
    public DbSet<Mandat> Mandaty => Set<Mandat>();
    public DbSet<ZdarzenieMandatowe> ZdarzeniaMandatowe => Set<ZdarzenieMandatowe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Politycy
        modelBuilder.Entity<Polityk>(b =>
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

        modelBuilder.Entity<Wybory>()
            .HasOne(x => x.Rodzaj)
            .WithMany(z => z.Wybory)
            .HasForeignKey(x => x.RodzajWyborowId);

        modelBuilder.Entity<OkregWyborczy>(b =>
        {
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SzczegolyOkregu>(b =>
        {
            b.HasKey(x => new { x.OkregId, x.WyboryId });
        });

        modelBuilder.Entity<SzczegolyOkregu>()
            .HasOne(x => x.Okreg)
            .WithMany(w => w.Ludnosc)
            .HasForeignKey(x => x.OkregId);

        modelBuilder.Entity<SzczegolyOkregu>()
            .HasOne(x => x.Wybory)
            .WithMany()
            .HasForeignKey(x => x.WyboryId);

        modelBuilder.Entity<PartiaCzlonkostwo>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.PolitykId)
                .IsRequired();

            b.Property(x => x.PartiaId)
                .IsRequired();

            b.Property(x => x.WyboryId)
                .IsRequired();

            // FK -> Polityk
            b.HasOne<Polityk>()
                .WithMany(x => x.Czlonkostwa)
                .HasForeignKey(x => x.PolitykId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK -> Klub
            b.HasOne<Partia>()
                .WithMany(x => x.Czlonkostwa)
                .HasForeignKey(x => x.PartiaId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK -> Wybory
            b.HasOne<Wybory>()
                .WithMany(x => x.Czlonkostwa)
                .HasForeignKey(x => x.WyboryId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        base.OnModelCreating(modelBuilder);
    }
}
