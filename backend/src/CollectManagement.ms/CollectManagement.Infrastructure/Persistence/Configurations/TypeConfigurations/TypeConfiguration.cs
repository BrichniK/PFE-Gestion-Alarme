using CollectManagement.Domain.Types.ValueObjects;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Infrastructure.Persistence.Configurations.TypeConfigurations;

public class TypeConfiguration : IEntityTypeConfiguration<Type>
{
    public void Configure(EntityTypeBuilder<Type> builder)
    {
        builder.HasKey(d => d.TypeId);

        builder.Property(d => d.TypeId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new TypeId(new Ulid(value)));
        
        builder.Property(d => d.Code)
            .HasColumnType("nvarchar(200)")
            .IsRequired();
        
        builder.Property(d => d.Label)
            .HasColumnType("nvarchar(200)")
            .IsRequired();
        
        builder.Property(d => d.DureeNominal)
            .IsRequired(false);
    }
}