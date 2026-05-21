using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class WeeklyPlanConfiguration : AuditableEntityConfiguration<WeeklyPlan, Guid>
{
    public override void Configure(EntityTypeBuilder<WeeklyPlan> builder)
    {
        base.Configure(builder);
        builder.ToTable("weekly_plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.WeekId).HasMaxLength(16).IsRequired();
        builder.HasIndex(p => p.WeekId).IsUnique();
        builder.Property(p => p.WeekDatesJson).HasColumnName("week_dates_json").IsRequired();
        builder.Property(p => p.PublishedAtUtc).HasColumnName(DbNaming.Columns.PublishedAt);

        builder.HasMany(p => p.Slots)
            .WithOne()
            .HasForeignKey(s => s.WeeklyPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
