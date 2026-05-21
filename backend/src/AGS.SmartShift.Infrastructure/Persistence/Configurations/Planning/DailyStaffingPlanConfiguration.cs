using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class DailyStaffingPlanConfiguration : AuditableEntityConfiguration<DailyStaffingPlan, Guid>
{
    public override void Configure(EntityTypeBuilder<DailyStaffingPlan> builder)
    {
        base.Configure(builder);
        builder.ToTable("daily_staffing_plans");
        builder.Property(p => p.WeekId).HasMaxLength(16).IsRequired();
        builder.Property(p => p.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(p => p.HeaderJson).HasColumnType("jsonb");
        builder.Property(p => p.ConfirmedAtUtc).HasColumnName("confirmed_at");
        builder.HasIndex(p => new { p.SiteId, p.WeekId, p.DayIdx, p.DepartmentCode })
            .IsUnique()
            .HasDatabaseName("uq_daily_staffing_plans_site_week_day_dept");
    }
}
