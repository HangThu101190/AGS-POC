using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class ShiftSyncProposalConfiguration : AuditableEntityConfiguration<ShiftSyncProposal, Guid>
{
    public override void Configure(EntityTypeBuilder<ShiftSyncProposal> builder)
    {
        base.Configure(builder);
        builder.ToTable("shift_sync_proposals");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.WeekId).HasMaxLength(16).IsRequired();
        builder.Property(p => p.FlightNo).HasMaxLength(16).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.AffectedJson).HasColumnName("affected_json").IsRequired();
        builder.Property(p => p.ConfirmedAtUtc).HasColumnName(DbNaming.Columns.ConfirmedAt);
        builder.Property(p => p.Summary).HasMaxLength(512);
        builder.HasIndex(p => new { p.FlightId, p.Status });
    }
}
