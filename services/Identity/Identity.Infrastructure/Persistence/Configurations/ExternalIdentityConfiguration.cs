using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

internal sealed class ExternalIdentityConfiguration
    : IEntityTypeConfiguration<ExternalIdentity>
{
    public void Configure(
        EntityTypeBuilder<ExternalIdentity> builder)
    {
        builder.ToTable("external_identities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.Provider,
                    x.ProviderUserId
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_external_identities_provider_user_id");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName(
                "ix_external_identities_user_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
