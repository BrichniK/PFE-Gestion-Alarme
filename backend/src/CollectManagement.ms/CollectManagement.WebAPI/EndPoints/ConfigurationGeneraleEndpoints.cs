using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.ConfigurationGenerales.Commands.UpdateConfigurationGenerale;
using CollectManagement.Application.Features.ConfigurationGenerales.Queries.GetConfigurationGenerale;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class ConfigurationGeneraleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/configuration-generale").RequireAuthorization();

        routeGroupBuilder.MapGet("get", GetConfiguration);
        routeGroupBuilder.MapPost("update", UpdateConfiguration);
    }

    private static async Task<IResult> GetConfiguration(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetConfigurationGeneraleQuery(), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetConfigurationGeneraleResponse>(response));
    }

    private static async Task<IResult> UpdateConfiguration(
        [FromBody][Required] UpdateConfigurationGeneraleCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<UpdateConfigurationGeneraleResponse>(response));
    }
}
