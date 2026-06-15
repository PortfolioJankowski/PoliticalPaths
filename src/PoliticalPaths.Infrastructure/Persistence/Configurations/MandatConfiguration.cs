using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Kadencje;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class MandatConfiguration : IEntityTypeConfiguration<Mandat>
{
    public void Configure(EntityTypeBuilder<Mandat> builder)
    {
        builder.ToTable("Mandaty");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TypObjecia).HasConversion<int>();

        builder.HasOne<PoliticalPaths.Domain.Politycy.Polityk>()
            .WithMany(p => p.Mandaty)
            .HasForeignKey(x => x.PolitykId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PoliticalPaths.Domain.Wybory.Wybory>()
            .WithMany()
            .HasForeignKey(x => x.WyboryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PoliticalPaths.Domain.StartyWyborcze.StartWyborczy>()
            .WithMany()
            .HasForeignKey(x => x.StartWyborczyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
