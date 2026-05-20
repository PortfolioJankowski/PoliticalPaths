using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations;

public sealed class TransformationErrorConfiguration : IEntityTypeConfiguration<TransformationError>
{
    public void Configure(EntityTypeBuilder<TransformationError> builder)
    {
        builder.ToTable("TransformationErrors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StepName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ErrorCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FieldName).HasMaxLength(128);
        builder.Property(x => x.RawValue).HasMaxLength(512);
        builder.Property(x => x.DetailsJson).HasColumnType("json");
        builder.HasIndex(x => x.ImportRowId);
        builder.HasOne(x => x.ImportRow).WithMany(x => x.Errors).HasForeignKey(x => x.ImportRowId);
    }
}
