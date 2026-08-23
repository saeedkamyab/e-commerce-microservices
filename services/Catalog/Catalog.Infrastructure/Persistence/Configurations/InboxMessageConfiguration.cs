using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(
        EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ReceivedOnUtc)
            .HasColumnName("received_on_utc")
            .IsRequired();

        builder.Property(x => x.ProcessedOnUtc)
            .HasColumnName("processed_on_utc");

        builder.Property(x => x.Error)
            .HasColumnName("error");
    }
}
