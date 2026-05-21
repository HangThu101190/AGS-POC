using AGS.SmartShift.Domain.Entities.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class ShiftSlotConfiguration : IEntityTypeConfiguration<ShiftSlot>
{
    public void Configure(EntityTypeBuilder<ShiftSlot> builder)
    {
        builder.ToTable("shift_slots");
        builder.Property(s => s.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(s => s.SegmentsJson).HasColumnName("segments_json").IsRequired();
        builder.Property(s => s.FlightNosJson).HasColumnName("flight_nos_json").IsRequired();
        builder.Property(s => s.BucketKey).HasMaxLength(32);
        builder.HasIndex(s => new { s.WeeklyPlanId, s.DayIdx, s.DepartmentCode });
    }
}
