using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.SMSConfigurations.Commands.UpdateSMSConfiguration;
using CollectManagement.Application.Features.SMSConfigurations.Queries.GetSMSConfiguration;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class SMSConfigurationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/sms-configuration").RequireAuthorization();

        routeGroupBuilder.MapGet("get", GetConfiguration);
        routeGroupBuilder.MapPost("update", UpdateConfiguration);
    }

    private static async Task<IResult> GetConfiguration(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetSMSConfigurationQuery(), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetSMSConfigurationResponse>(response));
    }

    private static async Task<IResult> UpdateConfiguration(
        [FromBody][Required] UpdateSMSConfigurationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<UpdateSMSConfigurationResponse>(response));
    }
}
