using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

public sealed class SiteConfiguration : AuditableEntityConfiguration<Site, Guid>
{
    public override void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("sites");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(s => s.Timezone).HasColumnName("timezone").HasMaxLength(64).IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();

        base.Configure(builder);
    }
}
