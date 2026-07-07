using CollectManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace CollectManagement.Infrastructure.Services;

public class SignalService : ISignalService
{
    private readonly IHubContext<SignalRHub> _hubContext;

    public SignalService(IHubContext<SignalRHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMaintenanceUpdated()
    {
        await _hubContext.Clients.All.SendAsync("RefreshMaintenance");
        Console.WriteLine("✅ SignalR message RefreshMaintenance sent!");
    }

    public async Task NotifyMaintenanceCaptureUpdated(MaintenanceCaptureRealtimePayload payload)
    {
        await _hubContext.Clients.All.SendAsync("MaintenanceCaptureUpdated", payload);
    }

    public async Task NotifyDeviceCaptureStateChanged(DeviceCaptureStateRealtimePayload payload)
    {
        await _hubContext.Clients.All.SendAsync("DeviceCaptureStateChanged", payload);
    }

    public async Task NotifyDeviceStatusChanged(DeviceStatusPayload payload)
    {
        await _hubContext.Clients.All.SendAsync("DeviceStatusChanged", payload);
    }
}
