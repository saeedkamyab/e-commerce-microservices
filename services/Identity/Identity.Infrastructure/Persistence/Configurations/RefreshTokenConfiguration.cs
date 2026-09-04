using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiresOnUtc)
            .HasColumnName("expires_on_utc")
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(x => x.RevokedOnUtc)
            .HasColumnName("revoked_on_utc");

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();


        builder.Property(x => x.FamilyId)
    .HasColumnName("family_id")
    .IsRequired();

        builder.Property(x => x.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.FamilyId);


        builder.Ignore(x => x.IsRevoked);
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsActive);
    }
}