using AGS.SmartShift.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

internal sealed class SystemPermissionConfiguration : IEntityTypeConfiguration<SystemPermission>
{
    public void Configure(EntityTypeBuilder<SystemPermission> builder)
    {
        builder.ToTable("system_permissions");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(64);
        builder.Property(x => x.Module).HasColumnName("module").HasMaxLength(32).IsRequired();
        builder.Property(x => x.NameVi).HasColumnName("name_vi").HasMaxLength(256).IsRequired();
        builder.Property(x => x.NameEn).HasColumnName("name_en").HasMaxLength(256).IsRequired();
    }
}
