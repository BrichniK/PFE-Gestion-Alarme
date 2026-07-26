using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Maintenances.Queries.GetKpiIndicators;
using CollectManagement.Application.Features.Maintenances.Queries.GetMaintenanceStat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class StatEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/stat").RequireAuthorization();

        routeGroupBuilder.MapGet("list", StatList);
        routeGroupBuilder.MapGet("kpi-indicators", KpiIndicators);
    }

    private static async Task<IResult> StatList(
        [FromQuery] string? search,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var normalizedFromDate = fromDate?.Date;
        var normalizedToDateExclusive = toDate?.Date.AddDays(1);

        var list = await sender
            .Send(new GetMaintenanceStatQuery(search, null, null, page, size, normalizedFromDate, normalizedToDateExclusive), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetMaintenanceStatResponse>(list));
    }

    private static async Task<IResult> KpiIndicators(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Ulid? deviceId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetKpiIndicatorsQuery(startDate, endDate, deviceId), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetKpiIndicatorsResponse>(response));
    }
}
