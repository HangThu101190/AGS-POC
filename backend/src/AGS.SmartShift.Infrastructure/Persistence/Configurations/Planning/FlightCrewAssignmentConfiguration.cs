using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class FlightCrewAssignmentConfiguration : AuditableEntityConfiguration<FlightCrewAssignment, Guid>
{
    public override void Configure(EntityTypeBuilder<FlightCrewAssignment> builder)
    {
        base.Configure(builder);
        builder.ToTable("flight_crew_assignments");
        builder.HasOne<DailyStaffingLine>().WithMany().HasForeignKey(a => a.StaffingLineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(a => new { a.StaffingLineId, a.EmployeeId, a.Role });
    }
}
