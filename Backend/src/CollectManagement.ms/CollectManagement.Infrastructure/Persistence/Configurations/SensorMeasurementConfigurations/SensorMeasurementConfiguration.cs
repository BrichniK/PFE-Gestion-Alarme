using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.SensorMeasurementConfigurations;

public class SensorMeasurementConfiguration : IEntityTypeConfiguration<SensorMeasurement>
{
    public void Configure(EntityTypeBuilder<SensorMeasurement> builder)
    {
        builder.HasKey(d => d.SensorMeasurementId);

        builder.Property(d => d.SensorMeasurementId)
            .HasConversion(
                id => id.Value.ToGuid(),
                value => new SensorMeasurementId(new Ulid(value)));

        builder.Property(d => d.DeviceId)
            .HasConversion(
                id => id.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));

        builder.Property(d => d.SensorCode)
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.Property(d => d.MeasuredAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.Temperature)
            .HasColumnType("float")
            .IsRequired(false);

        builder.Property(d => d.Vibration)
            .HasColumnType("float")
            .IsRequired(false);

        builder.Property(d => d.Pressure)
            .HasColumnType("float")
            .IsRequired(false);

        builder.Property(d => d.Humidity)
            .HasColumnType("float")
            .IsRequired(false);

        builder.Property(d => d.IsFailure)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(d => new
        {
            d.DeviceId,
            d.MeasuredAt
        });
    }
}