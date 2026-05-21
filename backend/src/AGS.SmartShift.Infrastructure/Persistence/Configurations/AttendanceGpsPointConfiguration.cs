using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations;

public sealed class AttendanceGpsPointConfiguration : IEntityTypeConfiguration<AttendanceGpsPoint>
{
    public void Configure(EntityTypeBuilder<AttendanceGpsPoint> builder)
    {
        builder.ToTable(DbNaming.Tables.AttendanceLocationSamples);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttendanceRecordId).IsRequired();
        builder.Property(x => x.AtUtc).HasColumnName(DbNaming.Columns.RecordedAt).IsRequired();
        builder.Property(x => x.Lat).IsRequired();
        builder.Property(x => x.Lng).IsRequired();
        builder.HasIndex(x => new { x.AttendanceRecordId, x.AtUtc });
    }
}
