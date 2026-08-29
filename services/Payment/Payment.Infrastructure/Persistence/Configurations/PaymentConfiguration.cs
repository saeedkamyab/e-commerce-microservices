using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration
   : IEntityTypeConfiguration<Domain.Payments.Payment>
{
    public void Configure(
        EntityTypeBuilder<Domain.Payments.Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(x => x.CompletedOnUtc)
            .HasColumnName("completed_on_utc");

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}
