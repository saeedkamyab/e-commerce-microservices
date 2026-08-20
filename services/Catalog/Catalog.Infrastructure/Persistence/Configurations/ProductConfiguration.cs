using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

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
    .OnDelete(DeleteBehavior.Restrict);


        builder.Property(x => x.Description)
    .HasColumnName("description")
    .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        ConfigureName(builder);

        ConfigurePrice(builder);

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Specifications);
    }

    private static void ConfigureName(
        EntityTypeBuilder<Product> builder)
    {
        builder.OwnsOne(
            x => x.Name,
            name =>
            {
                name.Property(x => x.Value)
                    .HasColumnName("name")
                    .HasMaxLength(200)
                    .IsRequired();
            });
    }

    private static void ConfigurePrice(
        EntityTypeBuilder<Product> builder)
    {
        builder.OwnsOne(
            x => x.Price,
            price =>
            {
                price.Property(x => x.Amount)
                    .HasColumnName("price")
                    .HasPrecision(18, 2)
                    .IsRequired();

                price.Property(x => x.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
    }
}
