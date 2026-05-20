using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.HasIndex(x => new { x.Status, x.NextRetryAt });
        builder.HasOne(x => x.ImportBatch).WithMany(x => x.Jobs).HasForeignKey(x => x.ImportBatchId);
    }
}
