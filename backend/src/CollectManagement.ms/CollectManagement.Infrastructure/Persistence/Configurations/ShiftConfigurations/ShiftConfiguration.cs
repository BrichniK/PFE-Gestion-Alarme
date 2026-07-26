using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.ShiftConfigurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(d => d.ShiftId);

        builder.Property(d => d.ShiftId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new ShiftId(new Ulid(value)));
        
        builder.Property(d => d.Label)
            .HasColumnType("nvarchar(200)")
            .IsRequired();
        
        builder.Property(d => d.StartTime)
            .HasColumnType("time")
            .IsRequired();
        
        builder.Property(d => d.EndTime)
            .HasColumnType("time")
            .IsRequired();
    }
}