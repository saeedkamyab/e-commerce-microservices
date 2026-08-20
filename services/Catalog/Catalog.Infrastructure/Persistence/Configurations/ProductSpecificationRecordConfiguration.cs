using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductSpecificationRecordConfiguration
    : IEntityTypeConfiguration<ProductSpecificationRecord>
{
    public void Configure(
        EntityTypeBuilder<ProductSpecificationRecord> builder)
    {
        builder.ToTable("product_specifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.HasOne<Product>()
    .WithMany()
    .HasForeignKey(x => x.ProductId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AttributeDefinitionId)
            .HasColumnName("attribute_definition_id")
            .IsRequired();

        builder.HasOne<CategoryAttributeDefinitionRecord>()
    .WithMany()
    .HasForeignKey(x => x.AttributeDefinitionId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.TextValue)
            .HasColumnName("text_value")
            .HasMaxLength(500);

        builder.Property(x => x.NumberValue)
            .HasColumnName("number_value");

        builder.Property(x => x.DecimalValue)
            .HasColumnName("decimal_value")
            .HasPrecision(18, 4);

        builder.Property(x => x.BooleanValue)
            .HasColumnName("boolean_value");

        builder.Property(x => x.DateValue)
            .HasColumnName("date_value");

        builder.Property(x => x.OptionValue)
            .HasColumnName("option_value")
            .HasMaxLength(100);

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.AttributeDefinitionId
        })
        .IsUnique();
    }
}
