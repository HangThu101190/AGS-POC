using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class ShiftAssignmentConfiguration : AuditableEntityConfiguration<ShiftAssignment, Guid>
{
    public override void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        base.Configure(builder);
        builder.ToTable("shift_assignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FlightNosJson).HasColumnName("flight_nos_json").IsRequired();
        builder.Property(a => a.Status).HasMaxLength(32);
        builder.HasIndex(a => new { a.ShiftSlotId, a.EmployeeId }).IsUnique();
    }
}
