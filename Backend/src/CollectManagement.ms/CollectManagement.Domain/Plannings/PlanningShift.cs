using CollectManagement.Domain.Common;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Domain.Plannings;

public class PlanningShift : AuditableEntity
{
    public PlanningId PlanningId { get; private set; }

    public Planning Planning { get; private set; }

    public ShiftId ShiftId { get; private set; }

    public Shift Shift { get; private set; }

    private PlanningShift(
        PlanningId planningId,
        ShiftId shiftId)
    {
        PlanningId = planningId;
        ShiftId = shiftId;
    }

    public static PlanningShift Create(
        PlanningId planningId,
        ShiftId shiftId)
    {
        return new PlanningShift(planningId, shiftId);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PlanningShift() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
