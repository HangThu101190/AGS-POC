using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : AuditableEntityConfiguration<AttendanceRecord, Guid>
{
    public override void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        base.Configure(builder);
        builder.ToTable("attendance_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.CheckInLat);
        builder.Property(x => x.CheckInLng);
        builder.Property(x => x.CurrentLat);
        builder.Property(x => x.CurrentLng);
        builder.Property(x => x.InZone).IsRequired();
        builder.Property(x => x.GeoNote).HasMaxLength(500);
        builder.Property(x => x.LateCheckInNote).HasMaxLength(500);
        builder.Property(x => x.GeoSimulated).IsRequired();
        builder.HasIndex(x => x.FlightCrewAssignmentId);
        builder.Property(x => x.CheckInUtc).HasColumnName(DbNaming.Columns.CheckInAt);
        builder.Property(x => x.CheckOutUtc).HasColumnName(DbNaming.Columns.CheckOutAt);
        builder.HasIndex(x => x.EmployeeId);
    }
}
