using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class FlightScheduleDayConfiguration : AuditableEntityConfiguration<FlightScheduleDay, Guid>
{
    public override void Configure(EntityTypeBuilder<FlightScheduleDay> builder)
    {
        base.Configure(builder);
        builder.ToTable("flight_schedule_days");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.WeekId).HasMaxLength(16).IsRequired();
        builder.Property(d => d.SourceDayLabel).HasMaxLength(32);
        builder.Property(d => d.SheetRemark).HasColumnType("text");

        builder.HasIndex(d => new { d.WeekId, d.DayIdx })
            .IsUnique()
            .HasDatabaseName("ux_flight_schedule_days_week_day");

        builder.HasIndex(d => d.WeekId)
            .HasDatabaseName("ix_flight_schedule_days_week_id");
    }
}
