using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryAttributeDefinitionRecordConfiguration
    : IEntityTypeConfiguration<CategoryAttributeDefinitionRecord>
{
    public void Configure(
        EntityTypeBuilder<CategoryAttributeDefinitionRecord> builder)
    {
        builder.ToTable("category_attribute_definitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.HasOne<Category>()
    .WithMany()
    .HasForeignKey(x => x.CategoryId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CategoryId,
            x.Name
        })
        .IsUnique();
    }
}
