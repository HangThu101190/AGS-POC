using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class NotificationMessageConfiguration : AuditableEntityConfiguration<NotificationMessage, Guid>
{
    public override void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        base.Configure(builder);
        builder.ToTable(DbNaming.Tables.Notifications);
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).HasMaxLength(64).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(2000).IsRequired();
        builder.HasIndex(n => new { n.EmployeeId, n.IsRead });
    }
}
