using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations;

public sealed class WorkZoneConfiguration : AuditableEntityConfiguration<WorkZone, Guid>
{
    public override void Configure(EntityTypeBuilder<WorkZone> builder)
    {
        builder.ToTable("work_zones");

        builder.HasKey(z => z.Id);
        builder.Property(z => z.Id).HasColumnName("id");
        builder.Property(z => z.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(z => z.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(z => z.Version).HasColumnName("version").IsRequired();
        builder.Property(z => z.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(z => z.PolygonJson).HasColumnName("polygon_json").HasColumnType("jsonb").IsRequired();

        builder.HasIndex(z => new { z.SiteId, z.IsActive });

        base.Configure(builder);
    }
}
