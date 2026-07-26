using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.GroupeConfigurations;

public class GroupeConfiguration : IEntityTypeConfiguration<Groupe>
{
    public void Configure(EntityTypeBuilder<Groupe> builder)
    {
        builder.HasKey(g => g.GroupeId);

        builder.Property(g => g.GroupeId)
            .HasConversion(v => v.Value.ToGuid(),
                value => new GroupeId(new Ulid(value)));

        builder.Property(g => g.Nom)
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(g => g.Color)
            .HasColumnType("varchar(20)")
            .IsRequired(false);

        builder.Property(g => g.EmployeeIds)
            .HasConversion(
                v => string.Join(',', v.Select(id => id.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Ulid.Parse(s))
                    .ToList())
            .HasColumnType("nvarchar(max)");
    }
}
