using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Mqtt;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.WebAPI.EndPoints;

public class ResetEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/reset").RequireAuthorization();

        routeGroupBuilder.MapPost("{deviceId}", ResetDevice);
    }

    private static async Task<IResult> ResetDevice(
        [Required] Ulid deviceId,
        IDeviceRepository deviceRepository,
        IMaintenanceRepository maintenanceRepository,
        IMaintenanceCaptureHistoryRepository captureHistoryRepository,
        IMqttService mqttService,
        CancellationToken cancellationToken)
    {
        var typedDeviceId = new DeviceId(deviceId);

        var device = await deviceRepository
            .GetOneAsync(typedDeviceId, cancellationToken)
            .ConfigureAwait(false);

        if (device == null)
        {
            return Results.NotFound(new ApiResponse<bool>("Device not found"));
        }

        // Publish MQTT reset command
        var topic = $"ALARME/RESET/{device.Matricule}";
        await mqttService.PublishAsync(topic, "Reset");

        // Delete the last maintenance and its capture history for this device
        var lastMaintenance = await maintenanceRepository
            .GetLatestByDeviceIdAsync(typedDeviceId, cancellationToken)
            .ConfigureAwait(false);

        if (lastMaintenance != null)
        {
            // Delete capture history first (child records)
            await captureHistoryRepository
                .DeleteAsync(
                    c => c.MaintenanceId == lastMaintenance.MaintenanceId,
                    cancellationToken)
                .ConfigureAwait(false);

            // Delete the maintenance
            await maintenanceRepository
                .DeleteAsync(
                    m => m.MaintenanceId == lastMaintenance.MaintenanceId,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return Results.Ok(new ApiResponse<bool>(true));
    }
}
