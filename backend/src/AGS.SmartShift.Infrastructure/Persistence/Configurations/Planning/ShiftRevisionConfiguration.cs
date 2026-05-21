using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class ShiftRevisionConfiguration : AuditableEntityConfiguration<ShiftRevision, Guid>
{
    public override void Configure(EntityTypeBuilder<ShiftRevision> builder)
    {
        base.Configure(builder);
        builder.ToTable("shift_revisions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).HasMaxLength(512).IsRequired();
        builder.Property(r => r.FlightTrigger).HasMaxLength(16);
        builder.Property(r => r.ByCode).HasMaxLength(32).IsRequired();
        builder.Property(r => r.DeltaSegmentsJson).HasColumnName("delta_segments_json").IsRequired();
    }
}
