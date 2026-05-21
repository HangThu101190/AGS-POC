using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class AircraftManningRuleConfiguration : AuditableEntityConfiguration<AircraftManningRule, Guid>
{
    public override void Configure(EntityTypeBuilder<AircraftManningRule> builder)
    {
        base.Configure(builder);
        builder.ToTable("aircraft_manning_rules");
        builder.Property(r => r.AircraftPattern).HasMaxLength(16).IsRequired();
    }
}

public sealed class AirlineManningRuleConfiguration : AuditableEntityConfiguration<AirlineManningRule, Guid>
{
    public override void Configure(EntityTypeBuilder<AirlineManningRule> builder)
    {
        base.Configure(builder);
        builder.ToTable("airline_manning_rules");
        builder.Property(r => r.AirlinePrefix).HasMaxLength(8).IsRequired();
    }
}

public sealed class EmployeeDayAvailabilityConfiguration : AuditableEntityConfiguration<EmployeeDayAvailability, Guid>
{
    public override void Configure(EntityTypeBuilder<EmployeeDayAvailability> builder)
    {
        base.Configure(builder);
        builder.ToTable("employee_day_availabilities");
        builder.Property(a => a.WeekId).HasMaxLength(16).IsRequired();
        builder.HasIndex(a => new { a.WeekId, a.DayIdx, a.EmployeeId }).IsUnique();
    }
}

public sealed class FlightImportJobConfiguration : AuditableEntityConfiguration<FlightImportJob, Guid>
{
    public override void Configure(EntityTypeBuilder<FlightImportJob> builder)
    {
        base.Configure(builder);
        builder.ToTable("flight_import_jobs");
        builder.Property(j => j.WeekId).HasMaxLength(16).IsRequired();
        builder.Property(j => j.ErrorMessage).HasMaxLength(2000);
        builder.Property(j => j.ResultJson).HasColumnType("jsonb");
    }
}

public sealed class ShiftCheckInPolicyConfiguration : AuditableEntityConfiguration<ShiftCheckInPolicy, Guid>
{
    public override void Configure(EntityTypeBuilder<ShiftCheckInPolicy> builder)
    {
        base.Configure(builder);
        builder.ToTable("shift_check_in_policies");
        builder.Property(p => p.DepartmentCode).HasMaxLength(32);
        builder.Property(p => p.BucketKey).HasMaxLength(32).IsRequired();
        builder.Property(p => p.SegmentStart).HasMaxLength(8).IsRequired();
        builder.Property(p => p.SegmentEnd).HasMaxLength(8).IsRequired();
        builder.HasIndex(p => new { p.SiteId, p.DepartmentCode, p.BucketKey }).IsUnique();
    }
}

public sealed class LeaveTypeConfiguration : AuditableEntityConfiguration<LeaveType, Guid>
{
    public override void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        base.Configure(builder);
        builder.ToTable("leave_types");
        builder.Property(t => t.Code).HasMaxLength(8).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();
    }
}

public sealed class LeaveRequestConfiguration : AuditableEntityConfiguration<LeaveRequest, Guid>
{
    public override void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        base.Configure(builder);
        builder.ToTable("leave_requests");
        builder.HasOne<LeaveType>().WithMany().HasForeignKey(r => r.LeaveTypeId);
        builder.HasIndex(r => new { r.EmployeeId, r.FromDate, r.ToDate });
    }
}
