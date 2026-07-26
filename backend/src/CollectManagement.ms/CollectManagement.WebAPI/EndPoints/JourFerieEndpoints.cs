using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;
using CollectManagement.Application.Features.JoursFeries.Commands.DeleteJourFerie;
using CollectManagement.Application.Features.JoursFeries.Commands.UpdateJourFerie;
using CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;
using CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class JourFerieEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/jour-ferie").RequireAuthorization();

        routeGroupBuilder.MapGet("list", JourFerieList);
        routeGroupBuilder.MapPost("add", CreateJourFerie);
        routeGroupBuilder.MapPatch("update", UpdateJourFerie);
        routeGroupBuilder.MapPost("{id}/delete", DeleteJourFerie);
        routeGroupBuilder.MapGet("{id}/one", OneJourFerie);
    }

    public static async Task<IResult> CreateJourFerie(
        [FromBody][Required] CreateJourFerieCommand createJourFerieCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createJourFerieCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateJourFerieResponse>(createResponse));
    }

    private static async Task<IResult> JourFerieList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListJourFerieQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListJourFerieResponse>(list));
    }

    private static async Task<IResult> UpdateJourFerie(
        [FromBody][Required] UpdateJourFerieCommand updateJourFerieCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateJourFerieCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteJourFerie(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteJourFerieCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneJourFerie(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneJourFerieQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneJourFerieResponse>(response));
    }
}
