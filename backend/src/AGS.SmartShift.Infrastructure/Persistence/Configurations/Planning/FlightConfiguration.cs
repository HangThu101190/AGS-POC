using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Planning;

public sealed class FlightConfiguration : AuditableEntityConfiguration<Flight, Guid>
{
    public override void Configure(EntityTypeBuilder<Flight> builder)
    {
        base.Configure(builder);
        builder.ToTable("flights");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.WeekId).HasMaxLength(16).IsRequired();
        builder.Property(f => f.FlightNo).HasMaxLength(32).IsRequired();
        builder.Property(f => f.DepartureFlightNo).HasMaxLength(32);
        builder.Property(f => f.Route).HasMaxLength(64).IsRequired();
        builder.Property(f => f.Sta).HasMaxLength(8).IsRequired();
        builder.Property(f => f.Std).HasMaxLength(8).IsRequired();
        builder.Property(f => f.Eta).HasMaxLength(8);
        builder.Property(f => f.Etd).HasMaxLength(8);
        builder.Property(f => f.EtaDelayMinutes).HasColumnName("eta_delay_minutes");
        builder.Property(f => f.EtdDelayMinutes).HasColumnName("etd_delay_minutes");
        builder.Property(f => f.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(f => f.Registration).HasMaxLength(32);
        builder.Property(f => f.Aircraft).HasMaxLength(16);
        builder.Property(f => f.Carry).HasMaxLength(32);
        builder.Property(f => f.ManningExplain).HasMaxLength(512);
        builder.Property(f => f.VipNote).HasMaxLength(256);
        builder.Property(f => f.Gate).HasMaxLength(32);
        builder.Property(f => f.Belt).HasMaxLength(32);
        builder.Property(f => f.Parking).HasMaxLength(32);
        builder.Property(f => f.Remark).HasColumnType("text");
        builder.Property(f => f.ExcelRowNo).HasColumnName("excel_row_no");
        builder.Property(f => f.SortOrder).HasColumnName("sort_order");

        builder.HasIndex(f => new { f.WeekId, f.DayIdx, f.SortOrder })
            .HasDatabaseName("ix_flights_week_day_sort");
        builder.HasIndex(f => new { f.WeekId, f.DayIdx })
            .HasDatabaseName("ix_flights_week_day");
        builder.HasIndex(f => new { f.WeekId, f.DayIdx, f.FlightNo })
            .HasDatabaseName("ix_flights_week_day_flight_no");
        builder.HasIndex(f => f.FlightNo);
    }
}
