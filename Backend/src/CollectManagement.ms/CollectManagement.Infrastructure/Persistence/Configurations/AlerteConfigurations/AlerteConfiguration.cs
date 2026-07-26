using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.AlerteConfigurations;

public class AlerteConfiguration : IEntityTypeConfiguration<Alerte>
{
    public void Configure(EntityTypeBuilder<Alerte> builder)
    {
        builder.HasKey(d => d.AlerteId);

        builder.Property(d => d.AlerteId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new AlerteId(new Ulid(value)));
        
        builder.Property(d => d.TypeId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new TypeId(new Ulid(value)));
        
        builder.Property(d => d.DispositifId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new DeviceId(new Ulid(value)));
        
        builder.Property(d => d.Date)
            .HasColumnType("datetime2")
            .IsRequired(false);
        
        builder.HasOne(e => e.Dispositif)
            .WithMany()
            .HasForeignKey(c => c.DispositifId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Type)
            .WithMany()
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Restrict); 
        
        builder.Property(d => d.Traiter)
            .HasDefaultValue(false);
        
    }
}