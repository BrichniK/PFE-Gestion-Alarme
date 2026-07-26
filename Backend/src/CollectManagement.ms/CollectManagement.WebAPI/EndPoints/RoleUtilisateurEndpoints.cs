using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.RolesUtilisateur.Commands.CreateRoleUtilisateur;
using CollectManagement.Application.Features.RolesUtilisateur.Commands.DeleteRoleUtilisateur;
using CollectManagement.Application.Features.RolesUtilisateur.Commands.UpdateRoleUtilisateur;
using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetAllRoleUtilisateur;
using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetListRoleUtilisateur;
using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetOneRoleUtilisateur;
using CollectManagement.Domain.Utilisateurs.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class RoleUtilisateurEndpoints :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/role-utilisateur").RequireAuthorization();

        routeGroupBuilder.MapGet("list", RoleList);
        routeGroupBuilder.MapGet("all", AllRole);
        routeGroupBuilder.MapGet("actions", NavigationActions);
        routeGroupBuilder.MapGet("{id}/one", OneRole);
        routeGroupBuilder.MapPost("", CreateRole);
        routeGroupBuilder.MapPatch("", UpdateRole);
        routeGroupBuilder.MapPost("{id}/delete", DeleteRole);
    }

    private static async Task<IResult> RoleList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        [FromQuery] int page,
        [FromQuery] int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetListRoleUtilisateurQuery(
                    search,
                    sort,
                    order,
                    page,
                    size), 
                cancellationToken);

        return Results.Ok(new ApiResponse<GetListRoleutilisateurResponse>(list));
    }
    
    private static async Task<IResult> AllRole(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetAllRoleUtilisateurQuery(), 
                cancellationToken);

        return Results.Ok(new ApiResponse<List<GetAllRoleUtilisateurResponse>>(list));
    }
    
    private static async Task<IResult> OneRole(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneRoleUtilisateurQuery(id), cancellationToken);

        return Results.Ok(new ApiResponse<GetOneRoleUtilisateurResponse>(response));
    }
    
    private static async Task<IResult> CreateRole(
        [FromBody][Required] CreateRoleUtilisateurCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);
    
        return Results.Ok(new ApiResponse<CreateRoleUtilisateurResponse>(createResponse));
    }
    
    private static async Task<IResult> UpdateRole(
        [FromBody][Required] UpdateRoleUtilisateurCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);
    
        return Results.Ok(new ApiResponse<bool>(true));
    }
    
    private static async Task<IResult> DeleteRole(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteRoleUtilisateurCommand(id), cancellationToken);

        return Results.Ok(new ApiResponse<bool>(true));
    }
    
    private static IResult NavigationActions()
    {
        var smartEnumToList = EnumHelper.SmartEnumToList<NavigationAction>();

        return Results.Ok(new ApiResponse<List<EnumInfo>>(smartEnumToList));
    }
}