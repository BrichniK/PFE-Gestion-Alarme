using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;
using CollectManagement.Application.Features.Maintenances.Commands.DeleteMaintenance;
using CollectManagement.Application.Features.Maintenances.Commands.UpdateMaintenance;
using CollectManagement.Application.Features.Maintenances.Queries.GetMonitoringStats;
using CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;
using CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Devices.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class MaintenanceEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/maintenance").RequireAuthorization();

        routeGroupBuilder.MapGet("list", MaintenanceList);
        routeGroupBuilder.MapGet("monitoring-stats", MonitoringStats);
        routeGroupBuilder.MapPost("add", CreateMaintenance);
        routeGroupBuilder.MapPatch("update", UpdateMaintenance);
        routeGroupBuilder.MapPost("{id}/delete", DeleteMaintenance);
        routeGroupBuilder.MapGet("{id}/one", OneMaintenance);
        routeGroupBuilder.MapPost("scan-rfid", ScanRfid);
        routeGroupBuilder.MapGet("{deviceId}/capture-history", DeviceCaptureHistory);
    }

    public static async Task<IResult> CreateMaintenance(
        [FromBody][Required] CreateMaintenanceCommand createMaintenanceCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createMaintenanceCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateMaintenanceResponse>(createResponse));
    }

    private static async Task<IResult> MaintenanceList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        [FromQuery] int page,
        [FromQuery] int size,
        [FromQuery] string? filter,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var normalizedFromDate = fromDate?.Date;
        var normalizedToDateExclusive = toDate?.Date.AddDays(1);

        var list = await sender
            .Send(
                new GetPagedListMaintenanceQuery(
                    search,
                    sort,
                    order,
                    page,
                    size,
                    filter,
                    normalizedFromDate,
                    normalizedToDateExclusive),
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListMaintenanceResponse>(list));
    }

    private static async Task<IResult> MonitoringStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Ulid? deviceId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var stats = await sender
            .Send(new GetMonitoringStatsQuery(startDate, endDate, deviceId), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetMonitoringStatsResponse>(stats));
    }

    private static async Task<IResult> UpdateMaintenance(
        [FromBody][Required] UpdateMaintenanceCommand updateMaintenanceCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateMaintenanceCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteMaintenance(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteMaintenanceCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneMaintenance(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneMaintenanceQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneMaintenanceResponse>(response));
    }

    /// <summary>
    /// Handles RFID tag scan for maintenance workflow.
    /// Matches RFID with employee, finds active maintenance, and advances T1→T2→T3→T4.
    /// </summary>
    public static async Task<IResult> ScanRfid(
        [FromBody] ScanRfidRequest request,
        IMaintenanceRfidService maintenanceRfidService,
        CancellationToken cancellationToken)
    {
        var response = await maintenanceRfidService
            .HandleRfidScanAsync(request.Rfid, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<MaintenanceRfidResponse>(response));
    }

    public static async Task<IResult> DeviceCaptureHistory(
        [Required] Ulid deviceId,
        [FromQuery] int page,
        [FromQuery] int size,
        IMaintenanceCaptureHistoryRepository maintenanceCaptureHistoryRepository,
        CancellationToken cancellationToken)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 50 : size;

        var (captures, length) = await maintenanceCaptureHistoryRepository
            .GetPagedByDeviceIdAsync(new DeviceId(deviceId), page, size, cancellationToken)
            .ConfigureAwait(false);

        var list = captures
            .Select(capture => new DeviceCaptureHistoryItem(
                capture.MaintenanceCaptureHistoryId.Value,
                capture.MaintenanceId.Value,
                capture.DeviceId.Value,
                capture.Device?.DeviceName,
                capture.Device?.Matricule,
                capture.EmployeeId.Value,
                capture.Employee?.Nom,
                capture.Employee?.Prenom,
                capture.TagId,
                capture.Step,
                capture.Status,
                capture.CapturedAt,
                capture.Maintenance?.T3Arrival,
                capture.Maintenance?.T4Completion))
            .ToList();

        return Results.Ok(
            new ApiResponse<DeviceCaptureHistoryResponse>(new DeviceCaptureHistoryResponse(list, length)));
    }
}

public record ScanRfidRequest(string Rfid);

public record DeviceCaptureHistoryItem(
    Ulid CaptureHistoryId,
    Ulid MaintenanceId,
    Ulid DeviceId,
    string? DeviceName,
    string? DeviceMatricule,
    Ulid EmployeeId,
    string? EmployeeNom,
    string? EmployeePrenom,
    string TagId,
    string Step,
    string Status,
    DateTime CapturedAt,
    DateTime? T3Arrival,
    DateTime? T4Completion
);

public record DeviceCaptureHistoryResponse(
    List<DeviceCaptureHistoryItem> Captures,
    int Length
);
