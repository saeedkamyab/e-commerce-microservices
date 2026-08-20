using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class AttributeOptionRecordConfiguration
    : IEntityTypeConfiguration<AttributeOptionRecord>
{
    public void Configure(
        EntityTypeBuilder<AttributeOptionRecord> builder)
    {
        builder.ToTable("attribute_options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.AttributeDefinitionId)
            .HasColumnName("attribute_definition_id")
            .IsRequired();

        builder.HasOne<CategoryAttributeDefinitionRecord>()
    .WithMany()
    .HasForeignKey(x => x.AttributeDefinitionId)
    .OnDelete(DeleteBehavior.Cascade);


        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AttributeDefinitionId,
            x.Value
        })
        .IsUnique();
    }
}
