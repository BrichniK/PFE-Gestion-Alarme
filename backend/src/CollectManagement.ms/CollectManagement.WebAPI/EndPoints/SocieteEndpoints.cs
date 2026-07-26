using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Societes.Commands.CreateSociete;
using CollectManagement.Application.Features.Societes.Commands.DeleteSociete;
using CollectManagement.Application.Features.Societes.Commands.UpdateSociete;
using CollectManagement.Application.Features.Societes.Queries.GetLogo;
using CollectManagement.Application.Features.Societes.Queries.GetOneSociete;
using CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class SocieteEndpoints :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/societe").RequireAuthorization();
        
        routeGroupBuilder.MapGet("list",SocieteList);
        routeGroupBuilder.MapPost("add",CreateSociete);
        routeGroupBuilder.MapPatch("update", UpdateSociete);
        routeGroupBuilder.MapPost("{id}/delete", DeleteSociete);
        routeGroupBuilder.MapGet("images/{logoName}", GetLogo);
        routeGroupBuilder.MapGet("{id}/one", OneSociete);
    }
    
    public static async Task<IResult> CreateSociete(
        [FromBody] [Required] CreateSocieteCommand createSocieteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createSocieteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateSocieteReponse>(createResponse));
    }

    private static async Task<IResult> SocieteList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListSocieteQuery(search, sort, order,page,size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListSocieteResponse>(list));
    }

    private static async Task<IResult> UpdateSociete(
        [FromBody] [Required] UpdateSocieteCommand updateSocieteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateSocieteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteSociete(
        [FromQuery] [Required] Ulid societeId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteSocieteCommand(societeId), cancellationToken)
            .ConfigureAwait(false);
        
        return Results.Ok(new ApiResponse<bool>(true));
    }
    
    public async static Task<IResult> GetLogo(
        [Required] string logoName,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetLogoQuery(logoName), cancellationToken)
            .ConfigureAwait(false);

        return Results.File(response.Logo, "image/jpeg");
    }
    
    public async static Task<IResult> OneSociete(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneSocieteQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneSocieteResponse>(response));
    }
}