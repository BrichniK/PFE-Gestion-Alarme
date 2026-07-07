using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Domain.Plannings;

public class Planning : AuditableEntity
{
    public PlanningId PlanningId { get; private set; }

    public DateTime Date { get; private set; }

    public ICollection<PlanningGroupe> PlanningGroupes { get; private set; } = new List<PlanningGroupe>();

    public ICollection<PlanningDevice> PlanningDevices { get; private set; } = new List<PlanningDevice>();

    public ICollection<PlanningShift> PlanningShifts { get; private set; } = new List<PlanningShift>();

    public ICollection<PlanningEmployee> PlanningEmployees { get; private set; } = new List<PlanningEmployee>();

    private Planning(
        PlanningId planningId,
        DateTime date,
        IEnumerable<GroupeId> groupeIds,
        IEnumerable<DeviceId> deviceIds,
        IEnumerable<ShiftId> shiftIds,
        IEnumerable<EmployeeId> employeeIds)
    {
        PlanningId = planningId;
        Date = date;
        SetGroupeRelations(groupeIds);
        SetDeviceRelations(deviceIds);
        SetShiftRelations(shiftIds);
        SetEmployeeRelations(employeeIds);
    }

    public static Planning Create(
        PlanningId planningId,
        DateTime date,
        IEnumerable<GroupeId> groupeIds,
        IEnumerable<DeviceId> deviceIds,
        IEnumerable<ShiftId> shiftIds,
        IEnumerable<EmployeeId> employeeIds)
    {
        return new Planning(
            planningId,
            date,
            groupeIds,
            deviceIds,
            shiftIds,
            employeeIds);
    }

    public void Update(
        DateTime date,
        IEnumerable<GroupeId> groupeIds,
        IEnumerable<DeviceId> deviceIds,
        IEnumerable<ShiftId> shiftIds,
        IEnumerable<EmployeeId> employeeIds)
    {
        Date = date;
        SetGroupeRelations(groupeIds);
        SetDeviceRelations(deviceIds);
        SetShiftRelations(shiftIds);
        SetEmployeeRelations(employeeIds);
    }

    private void SetGroupeRelations(IEnumerable<GroupeId> groupeIds)
    {
        PlanningGroupes = groupeIds
            .Distinct()
            .Select(groupeId => PlanningGroupe.Create(PlanningId, groupeId))
            .ToList();
    }

    private void SetDeviceRelations(IEnumerable<DeviceId> deviceIds)
    {
        PlanningDevices = deviceIds
            .Distinct()
            .Select(deviceId => PlanningDevice.Create(PlanningId, deviceId))
            .ToList();
    }

    private void SetShiftRelations(IEnumerable<ShiftId> shiftIds)
    {
        PlanningShifts = shiftIds
            .Distinct()
            .Select(shiftId => PlanningShift.Create(PlanningId, shiftId))
            .ToList();
    }

    private void SetEmployeeRelations(IEnumerable<EmployeeId> employeeIds)
    {
        PlanningEmployees = employeeIds
            .Distinct()
            .Select(employeeId => PlanningEmployee.Create(PlanningId, employeeId))
            .ToList();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Planning() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
