using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ImportFileConfiguration : IEntityTypeConfiguration<ImportFile>
{
    public void Configure(EntityTypeBuilder<ImportFile> builder)
    {
        builder.ToTable("ImportFiles");
        builder.HasKey(x => x.Id);

        var logicalNamesComparer = new ValueComparer<string[]>(
            (a, b) => (a != null && b != null && a.SequenceEqual(b)) || (a == null && b == null),
            a => a == null ? 0 : a.Aggregate(0, (h, v) => h ^ (v == null ? 0 : v.GetHashCode())),
            a => a == null ? new string[0] : a.ToArray()
        );

        builder.Property(x => x.LogicalNames)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(512)
            .IsRequired()
            .Metadata.SetValueComparer(logicalNamesComparer);

        builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FormatVersion).HasMaxLength(16).IsRequired();
        builder.Property(x => x.LogFilePath).HasMaxLength(1024);
        builder.HasIndex(x => new { x.ImportBatchId, x.Sha256 });
        builder.HasOne(x => x.ImportBatch).WithMany(x => x.Files).HasForeignKey(x => x.ImportBatchId);
    }
}
