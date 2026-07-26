using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS;
using CollectManagement.Domain.SMS.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectManagement.Infrastructure.Persistence.Configurations.SMSConfigurations;

public class SMSDeviceConfiguration : IEntityTypeConfiguration<SMSDevice>
{
    public void Configure(EntityTypeBuilder<SMSDevice> builder)
    {
        builder.ToTable("SMSDevice");
        
        builder.HasKey(x => new { x.SMSId, x.DeviceId });
        
        builder.Property(d => d.SMSId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new SMSId(new Ulid(value)));
        
        builder.Property(d => d.DeviceId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));
        
        builder.HasIndex(x => x.DeviceId);
        
        builder.HasOne(sd => sd.SMS)
            .WithMany(s => s.SMSDevices)
            .HasForeignKey(sd => sd.SMSId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(sd => sd.Device)
            .WithMany()
            .HasForeignKey(sd => sd.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
