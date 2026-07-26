using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances.ObjectValues;


namespace CollectManagement.Domain.Maintenances;

public class MaintenanceCaptureHistory : AuditableEntity
{
    public MaintenanceCaptureHistoryId MaintenanceCaptureHistoryId { get; private set; }

    public MaintenanceId MaintenanceId { get; private set; }

    public Maintenance Maintenance { get; private set; }

    public DeviceId DeviceId { get; private set; }

    public Device Device { get; private set; }

    public EmployeeId EmployeeId { get; private set; }

    public Employee Employee { get; private set; }

    public string TagId { get; private set; }

    public string Step { get; private set; }

    public string Status { get; private set; }

    public DateTime CapturedAt { get; private set; }

    private MaintenanceCaptureHistory(
        MaintenanceCaptureHistoryId maintenanceCaptureHistoryId,
        MaintenanceId maintenanceId,
        DeviceId deviceId,
        EmployeeId employeeId,
        string tagId,
        string step,
        string status,
        DateTime capturedAt)
    {
        MaintenanceCaptureHistoryId = maintenanceCaptureHistoryId;
        MaintenanceId = maintenanceId;
        DeviceId = deviceId;
        EmployeeId = employeeId;
        TagId = tagId;
        Step = step;
        Status = status;
        CapturedAt = capturedAt;
    }

    public static MaintenanceCaptureHistory Create(
        MaintenanceCaptureHistoryId maintenanceCaptureHistoryId,
        MaintenanceId maintenanceId,
        DeviceId deviceId,
        EmployeeId employeeId,
        string tagId,
        string step,
        string status,
        DateTime capturedAt)
    {
        return new MaintenanceCaptureHistory(
            maintenanceCaptureHistoryId,
            maintenanceId,
            deviceId,
            employeeId,
            tagId,
            step,
            status,
            capturedAt);
    }

#pragma warning disable CS8618
    private MaintenanceCaptureHistory()
    {
    }
#pragma warning restore CS8618
}
