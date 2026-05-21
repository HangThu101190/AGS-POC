using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserAccountId).HasColumnName(DbNaming.Columns.UserId);
        builder.Property(x => x.ExpiresAtUtc).HasColumnName(DbNaming.Columns.ExpiresAt);
        builder.Property(x => x.RevokedAtUtc).HasColumnName(DbNaming.Columns.RevokedAt);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.TokenHash);
        builder.HasIndex(x => new { x.UserAccountId, x.ExpiresAtUtc });
    }
}
