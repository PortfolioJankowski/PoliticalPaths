using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public class StartWyborczyConfiguration : IEntityTypeConfiguration<StartWyborczy>
{
    public void Configure(EntityTypeBuilder<StartWyborczy> builder)
    {
        builder
            .HasKey(x => x.Id);
        
        builder
            .HasOne(x => x.Polityk)
            .WithMany(p => p.StartyWyborcze)
            .HasForeignKey(x => x.PolitykId);
        
        builder
            .HasOne(x => x.Wybory)
            .WithMany()
            .HasForeignKey(x => x.WyboryId);
        
        builder
            .HasOne(x => x.Wyniki)
            .WithMany()
            .HasForeignKey(x => x.WynikiId);
        
        builder
            .HasOne(x => x.ListaWyborcza)
            .WithMany()
            .HasForeignKey(x => x.ListaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}