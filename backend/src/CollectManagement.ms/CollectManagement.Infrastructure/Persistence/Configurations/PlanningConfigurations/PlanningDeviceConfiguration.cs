using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.PlanningConfigurations;

public class PlanningDeviceConfiguration : IEntityTypeConfiguration<PlanningDevice>
{
    public void Configure(EntityTypeBuilder<PlanningDevice> builder)
    {
        builder.ToTable("PlanningDevice");

        builder.HasKey(x => new { x.PlanningId, x.DeviceId });

        builder.Property(d => d.PlanningId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new PlanningId(new Ulid(value)));

        builder.Property(d => d.DeviceId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));

        builder.HasIndex(x => x.DeviceId);

        builder.HasOne(pd => pd.Planning)
            .WithMany(p => p.PlanningDevices)
            .HasForeignKey(pd => pd.PlanningId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pd => pd.Device)
            .WithMany()
            .HasForeignKey(pd => pd.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
