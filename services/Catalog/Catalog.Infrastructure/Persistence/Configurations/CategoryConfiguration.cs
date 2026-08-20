using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ParentCategoryId)
            .HasColumnName("parent_category_id");

        builder.HasOne<Category>()
    .WithMany()
    .HasForeignKey(x => x.ParentCategoryId)
    .OnDelete(DeleteBehavior.Restrict);


        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(
            x => x.Name,
            name =>
            {
                name.Property(x => x.Value)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();
            });

        builder.Ignore(x => x.AttributeDefinitions);
    }
}
