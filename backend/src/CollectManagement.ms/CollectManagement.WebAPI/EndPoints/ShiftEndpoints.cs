using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Shifts.Commands.CreateShift;
using CollectManagement.Application.Features.Shifts.Commands.DeleteShift;
using CollectManagement.Application.Features.Shifts.Commands.UpdateShift;
using CollectManagement.Application.Features.Shifts.Queries.GetOneShift;
using CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class ShiftEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/shift").RequireAuthorization();

        routeGroupBuilder.MapGet("list", ShiftList);
        routeGroupBuilder.MapPost("add", CreateShift);
        routeGroupBuilder.MapPatch("update", UpdateShift);
        routeGroupBuilder.MapPost("{id}/delete", DeleteShift);
        routeGroupBuilder.MapGet("{id}/one", OneShift);
    }

    public static async Task<IResult> CreateShift(
        [FromBody][Required] CreateShiftCommand createShiftCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createShiftCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateShiftResponse>(createResponse));
    }

    private static async Task<IResult> ShiftList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListShiftQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListShiftResponse>(list));
    }

    private static async Task<IResult> UpdateShift(
        [FromBody][Required] UpdateShiftCommand updateShiftCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateShiftCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteShift(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteShiftCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneShift(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneShiftQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneShiftResponse>(response));
    }
}
