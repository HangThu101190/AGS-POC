using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

internal sealed class SystemRolePermissionConfiguration : IEntityTypeConfiguration<SystemRolePermission>
{
    public void Configure(EntityTypeBuilder<SystemRolePermission> builder)
    {
        builder.ToTable("system_role_permissions");
        builder.HasKey(x => new { x.RoleCode, x.PermissionCode });
        builder.Property(x => x.RoleCode).HasColumnName("role_code").HasMaxLength(16);
        builder.Property(x => x.PermissionCode).HasColumnName("permission_code").HasMaxLength(64);

        builder.HasOne<SystemRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleCode)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SystemPermission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
