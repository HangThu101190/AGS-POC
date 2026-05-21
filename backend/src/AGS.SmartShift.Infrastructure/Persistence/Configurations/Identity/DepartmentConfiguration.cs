using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

public sealed class DepartmentConfiguration : AuditableEntityConfiguration<Department, Guid>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(d => d.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(d => d.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(d => d.AllowedRoles)
            .HasColumnName("allowed_roles_json")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(AllowedRolesJsonConverter.Create())
            .Metadata.SetValueComparer(AllowedRolesJsonConverter.CreateComparer());

        builder.HasIndex(d => new { d.SiteId, d.Code }).IsUnique();

        builder.HasOne<Site>()
            .WithMany()
            .HasForeignKey(d => d.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        base.Configure(builder);
    }
}
