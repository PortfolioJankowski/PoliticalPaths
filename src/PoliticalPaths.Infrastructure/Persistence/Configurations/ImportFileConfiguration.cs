using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ImportFileConfiguration : IEntityTypeConfiguration<ImportFile>
{
    public void Configure(EntityTypeBuilder<ImportFile> builder)
    {
        builder.ToTable("ImportFiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LogicalName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FormatVersion).HasMaxLength(16).IsRequired();
        builder.Property(x => x.LogFilePath).HasMaxLength(1024);
        builder.HasIndex(x => new { x.ImportBatchId, x.Sha256 });
        builder.HasIndex(x => x.LogicalName);
        builder.HasOne(x => x.ImportBatch).WithMany(x => x.Files).HasForeignKey(x => x.ImportBatchId);
    }
}
