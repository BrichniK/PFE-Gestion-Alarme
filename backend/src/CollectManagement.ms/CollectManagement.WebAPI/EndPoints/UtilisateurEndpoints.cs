using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Utilisateurs.Commands.CreateUtilisateur;
using CollectManagement.Application.Features.Utilisateurs.Commands.DeleteUtilisateur;
using CollectManagement.Application.Features.Utilisateurs.Commands.UpdateUtilisateur;
using CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;
using CollectManagement.Domain.Utilisateurs.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class UtilisateurEndpoints : ICarterModule
{
   public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/utilisateur").RequireAuthorization();

        //routeGroupBuilder.MapPost("v1/create", CreateAdmin);
        routeGroupBuilder.MapPost("create", Create);
        routeGroupBuilder.MapGet("list", UtilisateurList);
        routeGroupBuilder.MapPatch("update", UpdateUtilisateur);
        routeGroupBuilder.MapGet("role", RoleList);
        routeGroupBuilder.MapPost("delete", DeleteUtilisateur);
    }

    private static async Task<IResult> Create(
        [FromBody][Required] CreateUtilisateurCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateUtilisateurResponse>(createResponse));
    }
    
    private static IResult RoleList()
    {
        var smartEnumToList = EnumHelper.SmartEnumToList<UtilisateurRole>();

        return Results.Ok(new ApiResponse<List<EnumInfo>>(smartEnumToList));
    }


    private static async Task<IResult> UtilisateurList(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetUtilisateurListQuery(search, sort, order, page, size), 
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetUtilisateurListResponse>(list));
    }
    
    public static async Task<IResult> UpdateUtilisateur(
        [FromBody][Required] UpdateUtilisateurCommand updateCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }
    
    public static async Task<IResult> DeleteUtilisateur(
        [FromBody][Required] DeleteUtilisateurCommand deleteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(deleteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<Boolean>(true));
    }
}