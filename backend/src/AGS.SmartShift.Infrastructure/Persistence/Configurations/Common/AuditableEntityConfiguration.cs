using AGS.SmartShift.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;

public abstract class AuditableEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity<TId>
    where TId : struct, IEquatable<TId>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("updated_at");

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken()
            .IsRequired();
    }
}
