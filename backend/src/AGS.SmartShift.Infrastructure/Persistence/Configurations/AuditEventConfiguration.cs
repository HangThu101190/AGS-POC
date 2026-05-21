using AGS.SmartShift.Domain.Entities.Audit;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OccurredAtUtc).HasColumnName(DbNaming.Columns.OccurredAt);
        builder.Property(x => x.UserAccountId).HasColumnName(DbNaming.Columns.UserId);
        builder.Property(x => x.HttpMethod).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(500).IsRequired();
        builder.Property(x => x.EmployeeCode).HasMaxLength(32);
        builder.HasIndex(x => x.OccurredAtUtc);
    }
}
