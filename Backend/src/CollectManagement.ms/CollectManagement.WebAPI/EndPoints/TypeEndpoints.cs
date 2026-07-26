using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Types.Commands.CreateType;
using CollectManagement.Application.Features.Types.Commands.DeleteType;
using CollectManagement.Application.Features.Types.Commands.UpdateType;
using CollectManagement.Application.Features.Types.Queries.GetOneType;
using CollectManagement.Application.Features.Types.Queries.GetPagedListType;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class TypeEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/type").RequireAuthorization();

        routeGroupBuilder.MapGet("list", TypeList);
        routeGroupBuilder.MapPost("add", CreateType);
        routeGroupBuilder.MapPatch("update", UpdateType);
        routeGroupBuilder.MapPost("{id}/delete", DeleteType);
        routeGroupBuilder.MapGet("{id}/one", OneType);
    }

    public static async Task<IResult> CreateType(
        [FromBody][Required] CreateTypeCommand createTypeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createTypeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateTypeResponse>(createResponse));
    }

    private static async Task<IResult> TypeList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListTypeQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListTypeResponse>(list));
    }

    private static async Task<IResult> UpdateType(
        [FromBody][Required] UpdateTypeCommand updateTypeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateTypeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteType(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteTypeCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneType(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneTypeQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneTypeResponse>(response));
    }
}
