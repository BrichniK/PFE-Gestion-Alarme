namespace CollectManagement.Application.Interfaces.Services;

public interface ISignalService
{
    Task NotifyMaintenanceUpdated();
    Task NotifyMaintenanceCaptureUpdated(MaintenanceCaptureRealtimePayload payload);
    Task NotifyDeviceCaptureStateChanged(DeviceCaptureStateRealtimePayload payload);
    Task NotifyDeviceStatusChanged(DeviceStatusPayload payload);
}
