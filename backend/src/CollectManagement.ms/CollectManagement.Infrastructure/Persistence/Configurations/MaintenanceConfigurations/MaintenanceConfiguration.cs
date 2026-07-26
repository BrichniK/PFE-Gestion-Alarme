using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Infrastructure.Persistence.Configurations.MaintenanceConfigurations;

public class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        builder.HasKey(p=>p.MaintenanceId);

        builder.Property(p => p.MaintenanceId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new MaintenanceId(new Ulid(value)));
        
        builder.Property(p => p.DeviceId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));
        
        builder.Property(p => p.EmployeeId)
            .HasConversion(p => p.Value.ToGuid(),
                value => new EmployeeId(new Ulid(value)));
        
        builder.Property(p=>p.T1Alerte)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.T2Assignment)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.T3Arrival)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.T4Completion)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.T5Confirmation)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.T6NextAlert)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.Property(p=>p.Description)
            .HasColumnType("varchar(500)")
            .IsRequired();
        
        builder.HasOne(e => e.Device)
            .WithMany()
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);     

    }
}