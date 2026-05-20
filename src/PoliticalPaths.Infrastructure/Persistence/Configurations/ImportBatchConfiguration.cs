using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PipelineKey).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.PipelineKey).IsUnique();
        builder.Property(x => x.TriggeredBy).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => x.Status);
    }
}
