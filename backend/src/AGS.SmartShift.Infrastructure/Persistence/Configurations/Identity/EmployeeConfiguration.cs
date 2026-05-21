using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

public sealed class EmployeeConfiguration : AuditableEntityConfiguration<Employee, Guid>
{
    public override void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.DepartmentId).HasColumnName("department_id").IsRequired();
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(e => e.ManagerId).HasColumnName("manager_id");
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.DefaultSegment).HasColumnName("default_segment").HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.PreferredShiftCode).HasColumnName("preferred_shift_code").HasMaxLength(20);
        builder.Property(e => e.MaxWeeklyHours).HasColumnName("max_weekly_hours").HasPrecision(4, 1).HasDefaultValue(48m);

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Name).HasDatabaseName("ix_employees_name");

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        base.Configure(builder);
    }
}
