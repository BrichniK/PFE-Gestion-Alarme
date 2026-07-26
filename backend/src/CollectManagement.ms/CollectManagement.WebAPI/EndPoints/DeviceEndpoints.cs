using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Devices.Commands.CreateDevice;
using CollectManagement.Application.Features.Devices.Commands.DeleteDevice;
using CollectManagement.Application.Features.Devices.Commands.UpdateDevice;
using CollectManagement.Application.Features.Devices.Queries.GetOneDevice;
using CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CollectManagement.WebAPI.EndPoints;

public class DeviceEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/device").RequireAuthorization();

        routeGroupBuilder.MapGet("list", DeviceList);
        routeGroupBuilder.MapPost("add", CreateDevice);
        routeGroupBuilder.MapPatch("update", UpdateDevice);
        routeGroupBuilder.MapPost("{id}/delete", DeleteDevice);
        routeGroupBuilder.MapGet("{id}/one", OneDevice);
        routeGroupBuilder.MapGet("{id}/capture-state", CaptureStateOneDevice);
        routeGroupBuilder.MapGet("capture-state/list", CaptureStateList);
    }

    public static async Task<IResult> CreateDevice(
        [FromBody][Required] CreateDeviceCommand createDeviceCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createDeviceCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateDeviceResponse>(createResponse));
    }

    private static async Task<IResult> DeviceList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListDeviceQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListDeviceResponse>(list));
    }

    private static async Task<IResult> UpdateDevice(
        [FromBody][Required] UpdateDeviceCommand updateDeviceCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateDeviceCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteDevice(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteDeviceCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneDevice(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneDeviceQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneDeviceResponse>(response));
    }

    public static async Task<IResult> CaptureStateOneDevice(
        [Required] Ulid id,
        IDeviceRepository deviceRepository,
        IAlerteRepository alerteRepository,
        IMaintenanceRepository maintenanceRepository,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetOneAsync(new DeviceId(id), cancellationToken).ConfigureAwait(false);
        if (device == null)
        {
            return Results.NotFound(new ApiResponse<string>("Device not found", false, 404));
        }

        var state = await BuildDeviceCaptureStateAsync(device, alerteRepository, maintenanceRepository, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<DeviceCaptureStateResponse>(new DeviceCaptureStateResponse(state)));
    }

    public static async Task<IResult> CaptureStateList(
        IDeviceRepository deviceRepository,
        IAlerteRepository alerteRepository,
        IMaintenanceRepository maintenanceRepository,
        CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var states = new List<DeviceCaptureStateItem>();
        foreach (var device in devices)
        {
            var state = await BuildDeviceCaptureStateAsync(device, alerteRepository, maintenanceRepository, cancellationToken)
                .ConfigureAwait(false);
            states.Add(state);
        }

        return Results.Ok(new ApiResponse<DeviceCaptureStateListResponse>(
            new DeviceCaptureStateListResponse(states, states.Count)));
    }

    private static async Task<DeviceCaptureStateItem> BuildDeviceCaptureStateAsync(
        Device device,
        IAlerteRepository alerteRepository,
        IMaintenanceRepository maintenanceRepository,
        CancellationToken cancellationToken)
    {
        var latestAlertsByCode = await alerteRepository
            .GetLatestUnprocessedCaptureAlertsByDeviceAsync(device.DeviceId, cancellationToken)
            .ConfigureAwait(false);
        var latestMaintenance = await maintenanceRepository
            .GetLatestByDeviceIdAsync(device.DeviceId, cancellationToken)
            .ConfigureAwait(false);

        var totalCaptures = Math.Max(0, device.NombreCapteur);
        var captureStatuses = new List<string>(totalCaptures);
        var captureLastErrorAt = new List<DateTime?>(totalCaptures);
        var captureAlertLabels = new List<string?>(totalCaptures);

        for (var captureIndex = 1; captureIndex <= totalCaptures; captureIndex++)
        {
            var code = $"A{captureIndex}";
            if (latestAlertsByCode.TryGetValue(code, out var latestAlert))
            {
                captureStatuses.Add("ERROR");
                captureLastErrorAt.Add(latestAlert.Date);
                captureAlertLabels.Add(latestAlert.Type?.Label ?? latestAlert.Type?.Code ?? code);
            }
            else
            {
                captureStatuses.Add("WORKING");
                captureLastErrorAt.Add(null);
                captureAlertLabels.Add(null);
            }
        }

        var workingCaptures = captureStatuses.Count(status => status == "WORKING");
        var capture1Status = totalCaptures >= 1 ? captureStatuses[0] : "NOT_AVAILABLE";
        var capture2Status = totalCaptures >= 2 ? captureStatuses[1] : "NOT_AVAILABLE";
        var capture1LastErrorAt = totalCaptures >= 1 ? captureLastErrorAt[0] : null;
        var capture2LastErrorAt = totalCaptures >= 2 ? captureLastErrorAt[1] : null;

        var maintenanceStartedAt = latestMaintenance?.T2Assignment ?? latestMaintenance?.T3Arrival ?? latestMaintenance?.T1Alerte;
        var maintenanceFinishedAt = latestMaintenance?.T5Confirmation;
        var isUnderMaintenance = latestMaintenance != null && !maintenanceFinishedAt.HasValue;
        var maintenancePhase = ResolveMaintenancePhase(latestMaintenance);
        var maintenancePhaseStartedAt = ResolveMaintenancePhaseStartedAt(latestMaintenance, maintenancePhase);
        var maintenanceCaptureIndex = isUnderMaintenance
            ? ResolveMaintenanceCaptureIndex(latestMaintenance, totalCaptures)
            : null;
        var maintenanceEmployeeName = latestMaintenance != null
            ? $"{latestMaintenance.Employee?.Nom ?? string.Empty} {latestMaintenance.Employee?.Prenom ?? string.Empty}".Trim()
            : null;

        if (isUnderMaintenance && !maintenanceCaptureIndex.HasValue)
        {
            var fallbackProcessedAlert = await alerteRepository
                .GetLatestProcessedCaptureAlertByDeviceBeforeAsync(
                    device.DeviceId,
                    maintenanceStartedAt,
                    cancellationToken)
                .ConfigureAwait(false);

            if (TryGetCaptureIndex(fallbackProcessedAlert?.Type?.Code, out var fallbackIndex)
                && fallbackIndex <= totalCaptures)
            {
                maintenanceCaptureIndex = fallbackIndex;
            }
        }

        var timeline = captureLastErrorAt
            .Concat(new DateTime?[] { maintenanceStartedAt, maintenanceFinishedAt });

        var lastUpdatedAt = timeline
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .DefaultIfEmpty(DateTime.UtcNow)
            .Max();

        return new DeviceCaptureStateItem(
            device.DeviceId.Value,
            device.DeviceName,
            device.Matricule,
            totalCaptures,
            workingCaptures,
            capture1Status,
            capture2Status,
            capture1LastErrorAt,
            capture2LastErrorAt,
            captureStatuses,
            captureLastErrorAt,
            captureAlertLabels,
            maintenanceCaptureIndex,
            isUnderMaintenance,
            maintenancePhase,
                maintenancePhaseStartedAt,
            maintenanceStartedAt,
            maintenanceFinishedAt,
            maintenanceEmployeeName,
            lastUpdatedAt);
    }

    private static string? ResolveMaintenancePhase(Maintenance? maintenance)
    {
        if (maintenance == null)
        {
            return null;
        }

        if (maintenance.T5Confirmation.HasValue)
        {
            return null;
        }

        if (maintenance.T3Arrival.HasValue && maintenance.T4Completion.HasValue)
        {
            return "REPARATION";
        }

        if (maintenance.T3Arrival.HasValue)
        {
            return "DIAGNOSTIC";
        }

        return "AFFECTEE";
    }

    private static DateTime? ResolveMaintenancePhaseStartedAt(Maintenance? maintenance, string? phase)
    {
        if (maintenance == null || string.IsNullOrWhiteSpace(phase))
        {
            return null;
        }

        var normalized = phase.ToUpperInvariant();
        if (normalized == "AFFECTEE")
        {
            return maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        if (normalized == "DIAGNOSTIC")
        {
            return maintenance.T3Arrival ?? maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        if (normalized == "REPARATION")
        {
            return maintenance.T4Completion ?? maintenance.T3Arrival ?? maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        return null;
    }

    private static int? ResolveMaintenanceCaptureIndex(Maintenance? maintenance, int totalCaptures)
    {
        if (maintenance == null || totalCaptures <= 0 || string.IsNullOrWhiteSpace(maintenance.Description))
        {
            return null;
        }

        const string prefix = "CAPTURE_CODE:";
        if (!maintenance.Description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var code = maintenance.Description[prefix.Length..].Trim();
        if (!TryGetCaptureIndex(code, out var captureIndex))
        {
            return null;
        }

        return captureIndex <= totalCaptures ? captureIndex : null;
    }

    private static bool TryGetCaptureIndex(string? code, out int captureIndex)
    {
        captureIndex = 0;
        if (string.IsNullOrWhiteSpace(code) || code.Length < 2 || !code.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(code[1..], NumberStyles.None, CultureInfo.InvariantCulture, out captureIndex)
               && captureIndex > 0;
    }
}

public record DeviceCaptureStateItem(
    Ulid DeviceId,
    string DeviceName,
    string DeviceMatricule,
    int TotalCaptures,
    int WorkingCaptures,
    string Capture1Status,
    string Capture2Status,
    DateTime? Capture1LastErrorAt,
    DateTime? Capture2LastErrorAt,
    IReadOnlyList<string> CaptureStatuses,
    IReadOnlyList<DateTime?> CaptureLastErrorAt,
    IReadOnlyList<string?> CaptureAlertLabels,
    int? MaintenanceCaptureIndex,
    bool IsUnderMaintenance,
    string? MaintenancePhase,
    DateTime? MaintenancePhaseStartedAt,
    DateTime? MaintenanceStartedAt,
    DateTime? MaintenanceFinishedAt,
    string? MaintenanceEmployeeName,
    DateTime LastUpdatedAt
);

public record DeviceCaptureStateResponse(
    DeviceCaptureStateItem State
);

public record DeviceCaptureStateListResponse(
    List<DeviceCaptureStateItem> Devices,
    int Length
);
