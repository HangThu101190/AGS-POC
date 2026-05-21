using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserAccountConfiguration : AuditableEntityConfiguration<UserAccount, Guid>
{
    public override void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        base.Configure(builder);
        builder.ToTable(DbNaming.Tables.Users);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LockoutEndUtc).HasColumnName(DbNaming.Columns.LockoutEndAt);
        builder.Property(x => x.LoginName).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.LoginName).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(5).HasDefaultValue("vi");
        builder.Property(x => x.MustChangePassword).HasColumnName("must_change_password").HasDefaultValue(false);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
