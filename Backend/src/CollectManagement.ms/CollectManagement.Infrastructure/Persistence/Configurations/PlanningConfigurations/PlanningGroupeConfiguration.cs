using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.PlanningConfigurations;

public class PlanningGroupeConfiguration : IEntityTypeConfiguration<PlanningGroupe>
{
    public void Configure(EntityTypeBuilder<PlanningGroupe> builder)
    {
        builder.ToTable("PlanningGroupe");

        builder.HasKey(x => new { x.PlanningId, x.GroupeId });

        builder.Property(d => d.PlanningId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new PlanningId(new Ulid(value)));

        builder.Property(d => d.GroupeId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new GroupeId(new Ulid(value)));

        builder.HasIndex(x => x.GroupeId);

        builder.HasOne(pg => pg.Planning)
            .WithMany(p => p.PlanningGroupes)
            .HasForeignKey(pg => pg.PlanningId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pg => pg.Groupe)
            .WithMany()
            .HasForeignKey(pg => pg.GroupeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
