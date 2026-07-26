using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;
using CollectManagement.Application.Features.Groupes.Commands.DeleteGroupe;
using CollectManagement.Application.Features.Groupes.Commands.UpdateGroupe;
using CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;
using CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class GroupeEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/groupe").RequireAuthorization();

        routeGroupBuilder.MapGet("list", GroupeList);
        routeGroupBuilder.MapPost("add", CreateGroupe);
        routeGroupBuilder.MapPatch("update", UpdateGroupe);
        routeGroupBuilder.MapPost("{id}/delete", DeleteGroupe);
        routeGroupBuilder.MapGet("{id}/one", OneGroupe);
    }

    public static async Task<IResult> CreateGroupe(
        [FromBody][Required] CreateGroupeCommand createGroupeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createGroupeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateGroupeResponse>(createResponse));
    }

    private static async Task<IResult> GroupeList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListGroupeQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListGroupeResponse>(list));
    }

    private static async Task<IResult> UpdateGroupe(
        [FromBody][Required] UpdateGroupeCommand updateGroupeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateGroupeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteGroupe(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteGroupeCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneGroupe(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneGroupeQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneGroupeResponse>(response));
    }
}
