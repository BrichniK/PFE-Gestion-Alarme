using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;
using CollectManagement.Application.Features.Plannings.Commands.DeletePlanning;
using CollectManagement.Application.Features.Plannings.Commands.UpdatePlanning;
using CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;
using CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class PlanningEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/planning").RequireAuthorization();

        routeGroupBuilder.MapGet("list", PlanningList);
        routeGroupBuilder.MapPost("add", CreatePlanning);
        routeGroupBuilder.MapPatch("update", UpdatePlanning);
        routeGroupBuilder.MapPost("{id}/delete", DeletePlanning);
        routeGroupBuilder.MapGet("{id}/one", OnePlanning);
    }

    public static async Task<IResult> CreatePlanning(
        [FromBody][Required] CreatePlanningCommand createPlanningCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createPlanningCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreatePlanningResponse>(createResponse));
    }

    private static async Task<IResult> PlanningList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListPlanningQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListPlanningResponse>(list));
    }

    private static async Task<IResult> UpdatePlanning(
        [FromBody][Required] UpdatePlanningCommand updatePlanningCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updatePlanningCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeletePlanning(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeletePlanningCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OnePlanning(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOnePlanningQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOnePlanningResponse>(response));
    }
}
