using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.PlanningConfigurations;

public class PlanningConfiguration : IEntityTypeConfiguration<Planning>
{
    public void Configure(EntityTypeBuilder<Planning> builder)
    {
        builder.HasKey(d => d.PlanningId);

        builder.Property(d => d.PlanningId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new PlanningId(new Ulid(value)));

        builder.Property(d => d.Date)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
