using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class ImportRowConfiguration : IEntityTypeConfiguration<ImportRow>
{
    public void Configure(EntityTypeBuilder<ImportRow> builder)
    {
        builder.ToTable("ImportRows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SheetName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RowHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RawPayloadJson).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.DomainEntityType).HasMaxLength(128);
        builder.Property(x => x.DomainEntityId).HasMaxLength(64);
        builder.HasIndex(x => new { x.ImportFileId, x.Status });
        builder.HasIndex(x => new { x.ImportFileId, x.SheetName, x.RowNumber }).IsUnique();
        builder.HasOne(x => x.ImportFile).WithMany(x => x.Rows).HasForeignKey(x => x.ImportFileId);
    }
}
