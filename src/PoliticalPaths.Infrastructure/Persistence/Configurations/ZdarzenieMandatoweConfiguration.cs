using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Kadencje;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ZdarzenieMandatoweConfiguration : IEntityTypeConfiguration<ZdarzenieMandatowe>
{
    public void Configure(EntityTypeBuilder<ZdarzenieMandatowe> builder)
    {
        builder.ToTable("ZdarzeniaMandatowe");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Typ).HasConversion<int>();
        builder.Property(x => x.Opis).HasMaxLength(1000);
        builder.Property(x => x.DokumentReferencyjny).HasMaxLength(256);

        builder.HasOne(z => z.Mandat)
            .WithMany(m => m.Zdarzenia)
            .HasForeignKey(x => x.MandatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(zd => zd.Polityk)
            .WithMany(p => p.ZdarzeniaMandatowe) 
            .HasForeignKey(x => x.PolitykId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
