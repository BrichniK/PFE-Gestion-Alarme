using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.PlanningConfigurations;

public class PlanningEmployeeConfiguration : IEntityTypeConfiguration<PlanningEmployee>
{
    public void Configure(EntityTypeBuilder<PlanningEmployee> builder)
    {
        builder.ToTable("PlanningEmployee");

        builder.HasKey(x => new { x.PlanningId, x.EmployeeId });

        builder.Property(d => d.PlanningId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new PlanningId(new Ulid(value)));

        builder.Property(d => d.EmployeeId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new EmployeeId(new Ulid(value)));

        builder.HasIndex(x => x.EmployeeId);

        builder.HasOne(pe => pe.Planning)
            .WithMany(p => p.PlanningEmployees)
            .HasForeignKey(pe => pe.PlanningId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.Employee)
            .WithMany()
            .HasForeignKey(pe => pe.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
