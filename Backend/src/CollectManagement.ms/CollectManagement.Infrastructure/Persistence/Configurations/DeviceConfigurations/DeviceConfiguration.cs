using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.DeviceConfigurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.DeviceId);

        builder.Property(d => d.DeviceId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));
        
        builder.Property(d => d.DeviceName)
            .HasColumnType("nvarchar(200)")
            .IsRequired();
        
        builder.Property(d => d.Matricule)
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.Property(d => d.NombreCapteur)
            .IsRequired();
    }
}
