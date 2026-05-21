using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class FlightScheduleConfiguration : AuditableEntityConfiguration<FlightSchedule, Guid>
{
    public override void Configure(EntityTypeBuilder<FlightSchedule> builder)
    {
        base.Configure(builder);
        builder.ToTable("flight_schedules");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.WeekId).HasMaxLength(16).IsRequired();
        builder.HasIndex(s => s.WeekId).IsUnique();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.PublishedAtUtc).HasColumnName(DbNaming.Columns.PublishedAt);
        builder.Property(s => s.LockedAtUtc).HasColumnName(DbNaming.Columns.LockedAt);
    }
}
