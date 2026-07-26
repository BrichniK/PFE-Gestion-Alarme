using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.JourFerieConfigurations;

public class JourFerieConfiguration : IEntityTypeConfiguration<JourFerie>
{
    public void Configure(EntityTypeBuilder<JourFerie> builder)
    {
        builder.HasKey(d => d.JourFerieId);

        builder.Property(d => d.JourFerieId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new JourFerieId(new Ulid(value)));

        builder.Property(d => d.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(d => d.Label)
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.HasIndex(d => d.Date).IsUnique();
    }
}
