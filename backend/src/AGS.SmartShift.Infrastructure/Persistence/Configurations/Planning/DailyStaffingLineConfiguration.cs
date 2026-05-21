using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class DailyStaffingLineConfiguration : AuditableEntityConfiguration<DailyStaffingLine, Guid>
{
    public override void Configure(EntityTypeBuilder<DailyStaffingLine> builder)
    {
        base.Configure(builder);
        builder.ToTable("daily_staffing_lines");
        builder.HasOne<Flight>().WithMany().HasForeignKey(l => l.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<DailyStaffingPlan>().WithMany().HasForeignKey(l => l.DailyStaffingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(l => new { l.DailyStaffingPlanId, l.FlightId, l.Segment })
            .IsUnique()
            .HasDatabaseName("uq_daily_staffing_lines_plan_flight_segment");
        builder.HasIndex(l => l.FlightId).HasDatabaseName("ix_daily_staffing_lines_flight_id");
    }
}
