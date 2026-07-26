using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Infrastructure.Persistence.Configurations.MaintenanceConfigurations;

public class MaintenanceCaptureHistoryConfiguration : IEntityTypeConfiguration<MaintenanceCaptureHistory>
{
    public void Configure(EntityTypeBuilder<MaintenanceCaptureHistory> builder)
    {
        builder.HasKey(p => p.MaintenanceCaptureHistoryId);

        builder.Property(p => p.MaintenanceCaptureHistoryId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new MaintenanceCaptureHistoryId(new Ulid(value)));

        builder.Property(p => p.MaintenanceId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new MaintenanceId(new Ulid(value)));

        builder.Property(p => p.DeviceId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));

        builder.Property(p => p.EmployeeId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new EmployeeId(new Ulid(value)));

        builder.Property(p => p.TagId)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(p => p.Step)
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnType("varchar(30)")
            .IsRequired();

        builder.Property(p => p.CapturedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(p => p.DeviceId);
        builder.HasIndex(p => p.CapturedAt);

        builder.HasOne(p => p.Maintenance)
            .WithMany()
            .HasForeignKey(p => p.MaintenanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Device)
            .WithMany()
            .HasForeignKey(p => p.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
