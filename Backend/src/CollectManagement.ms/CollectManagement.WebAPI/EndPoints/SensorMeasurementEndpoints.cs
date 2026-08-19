using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.SensorMeasurements.Analysis;
using CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;
using CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class SensorMeasurementEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app
            .MapGroup("cm/sensor-measurement");

        // Création d'une mesure
        routeGroupBuilder.MapPost(
            "add",
            CreateSensorMeasurement);

        // Liste paginée des mesures
        routeGroupBuilder.MapGet(
            "list",
            SensorMeasurementList);

        // Analyse historique d'une machine
        routeGroupBuilder.MapGet(
            "analysis/{deviceId}",
            GetSensorAnalysis);
    }

    // ============================================================
    // POST /cm/sensor-measurement/add
    // ============================================================

    public static async Task<IResult> CreateSensorMeasurement(
        [FromBody][Required]
        CreateSensorMeasurementCommand createSensorMeasurementCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(
                createSensorMeasurementCommand,
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(
            new ApiResponse<CreateSensorMeasurementResponse>(
                createResponse));
    }

    // ============================================================
    // GET /cm/sensor-measurement/list
    // ============================================================

    private static async Task<IResult> SensorMeasurementList(
        [FromQuery] Ulid? deviceId,
        [FromQuery] string? sensorCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page,
        [FromQuery] int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(
                new GetPagedListSensorMeasurementQuery(
                    deviceId,
                    sensorCode,
                    from,
                    to,
                    page,
                    size),
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(
            new ApiResponse<GetPagedListSensorMeasurementResponse>(
                response));
    }

    // ============================================================
    // GET /cm/sensor-measurement/analysis/{deviceId}
    // ============================================================

    private static async Task<IResult> GetSensorAnalysis(
        [FromRoute] string deviceId,
        [FromQuery] string? sensorCode,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Results.BadRequest(
                new ApiResponse<string>(
                    "DeviceId is required."));
        }

        // Le DeviceId de ton application est un ULID.
        if (!Ulid.TryParse(deviceId, out var ulid))
        {
            return Results.BadRequest(
                new ApiResponse<string>(
                    $"Invalid deviceId: '{deviceId}'. " +
                    "The deviceId must be a valid ULID."));
        }

        var response = await sender
            .Send(
                new GetSensorAnalysisQuery(
                    ulid,
                    sensorCode),
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(
            new ApiResponse<GetSensorAnalysisResponse>(
                response));
    }
}