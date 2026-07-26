using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Domain.Plannings;

public class PlanningDevice : AuditableEntity
{
    public PlanningId PlanningId { get; private set; }

    public Planning Planning { get; private set; }

    public DeviceId DeviceId { get; private set; }

    public Device Device { get; private set; }

    private PlanningDevice(
        PlanningId planningId,
        DeviceId deviceId)
    {
        PlanningId = planningId;
        DeviceId = deviceId;
    }

    public static PlanningDevice Create(
        PlanningId planningId,
        DeviceId deviceId)
    {
        return new PlanningDevice(planningId, deviceId);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PlanningDevice() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
