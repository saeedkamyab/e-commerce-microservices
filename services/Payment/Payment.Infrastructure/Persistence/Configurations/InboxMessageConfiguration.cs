using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Infrastructure.Persistence.Inbox;

namespace Payment.Infrastructure.Persistence.Configurations;

internal sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(
        EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.MessageId)
            .HasColumnName("message_id");

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
