using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Contracts.Authentication;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.Utilisateurs.Queries.Login;
using CollectManagement.Application.Features.Utilisateurs.Queries.LoginCheck;
using CollectManagement.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class AuthenticationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/authentication");
        
        routeGroupBuilder.MapPost("v1/login", LoginAdmin).AllowAnonymous();
        routeGroupBuilder.MapPost("v1/login-check", LoginAdminCheck).AllowAnonymous();
        routeGroupBuilder.MapPost("v99/login", LoginSuperAdmin).AllowAnonymous();
        routeGroupBuilder.MapPost("v99/login-check", LoginSuperAdminCheck).AllowAnonymous();
    }

    public static async Task<IResult> LoginSuperAdmin(
        [FromBody] [Required] LoginRequest loginRequest, 
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginQuery(
            loginRequest.Login, 
            loginRequest.Password);
        
        var authenticationResponse = await sender.Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<AuthenticationResponse>(authenticationResponse));
    }
    
    public static async Task<IResult> LoginAdmin(
        [FromBody] [Required] LoginRequest loginRequest, 
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginQuery(
            loginRequest.Login, 
            loginRequest.Password);
        
        var authenticationResponse = await sender.Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<AuthenticationResponse>(authenticationResponse));
    }
    
    public static async Task<IResult> LoginAdminCheck(
        [FromBody] [Required] LoginRequestCheck loginRequest, 
        ISender sender,
        ILoggedInUserService loggedInUserService,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(loggedInUserService.UserId, out var userId))
        {
            throw new NotFoundException("Utilisateur invalide.");
        }
        
        var command = new LoginCheckQuery(userId);
        
        var authenticationResponse = await sender.Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<AuthenticationResponse>(authenticationResponse));
    }

    private static async Task<IResult> LoginSuperAdminCheck(
        [FromBody] [Required] LoginRequestCheck loginRequest, 
        ISender sender,
        ILoggedInUserService loggedInUserService,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(loggedInUserService.UserId, out var userId))
        {
            throw new NotFoundException("Utilisateur invalide.");
        }
        
        var command = new LoginCheckQuery(userId);
        
        var authenticationResponse = await sender.Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<AuthenticationResponse>(authenticationResponse));
    }
    
}