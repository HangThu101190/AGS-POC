using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class ShiftTemplateConfiguration : AuditableEntityConfiguration<ShiftTemplate, Guid>
{
    public override void Configure(EntityTypeBuilder<ShiftTemplate> builder)
    {
        base.Configure(builder);
        builder.ToTable("shift_templates");
        builder.Property(t => t.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.MaxHours).HasPrecision(4, 1);
        builder.HasIndex(t => new { t.SiteId, t.DepartmentCode, t.Code })
            .IsUnique()
            .HasDatabaseName("uq_shift_templates_site_dept_code");
    }
}

public sealed class EmployeeQualificationConfiguration : AuditableEntityConfiguration<EmployeeQualification, Guid>
{
    public override void Configure(EntityTypeBuilder<EmployeeQualification> builder)
    {
        base.Configure(builder);
        builder.ToTable("employee_qualifications");
        builder.HasIndex(q => new { q.EmployeeId, q.Segment, q.CrewRole })
            .IsUnique()
            .HasDatabaseName("uq_employee_qualifications_emp_segment_role");
    }
}

public sealed class StaffingCrewProposalConfiguration : AuditableEntityConfiguration<StaffingCrewProposal, Guid>
{
    public override void Configure(EntityTypeBuilder<StaffingCrewProposal> builder)
    {
        base.Configure(builder);
        builder.ToTable("staffing_crew_proposals");
        builder.Property(p => p.Note).HasMaxLength(500);
        builder.HasOne<DailyStaffingPlan>()
            .WithMany()
            .HasForeignKey(p => p.DailyStaffingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<DailyStaffingLine>()
            .WithMany()
            .HasForeignKey(p => p.StaffingLineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.DailyStaffingPlanId)
            .HasDatabaseName("ix_staffing_crew_proposals_plan_id");
    }
}

public sealed class DailyStaffingBioHeaderConfiguration : AuditableEntityConfiguration<DailyStaffingBioHeader, Guid>
{
    public override void Configure(EntityTypeBuilder<DailyStaffingBioHeader> builder)
    {
        base.Configure(builder);
        builder.ToTable("daily_staffing_bio_headers");
        builder.Property(h => h.RadioNote).HasMaxLength(500);
        builder.HasOne<DailyStaffingPlan>()
            .WithMany()
            .HasForeignKey(h => h.DailyStaffingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => h.DailyStaffingPlanId)
            .IsUnique()
            .HasDatabaseName("uq_daily_staffing_bio_headers_plan_id");
    }
}
