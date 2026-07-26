using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.PlanningConfigurations;

public class PlanningShiftConfiguration : IEntityTypeConfiguration<PlanningShift>
{
    public void Configure(EntityTypeBuilder<PlanningShift> builder)
    {
        builder.ToTable("PlanningShift");

        builder.HasKey(x => new { x.PlanningId, x.ShiftId });

        builder.Property(d => d.PlanningId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new PlanningId(new Ulid(value)));

        builder.Property(d => d.ShiftId)
            .HasConversion(id => id.Value.ToGuid(),
                value => new ShiftId(new Ulid(value)));

        builder.HasIndex(x => x.ShiftId);

        builder.HasOne(ps => ps.Planning)
            .WithMany(p => p.PlanningShifts)
            .HasForeignKey(ps => ps.PlanningId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Shift)
            .WithMany()
            .HasForeignKey(ps => ps.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
