using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.SMS.Commands.CreateSMS;
using CollectManagement.Application.Features.SMS.Commands.DeleteSMS;
using CollectManagement.Application.Features.SMS.Commands.UpdateSMS;
using CollectManagement.Application.Features.SMS.Queries.GetSMSList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class SMSEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/sms").RequireAuthorization();

        routeGroupBuilder.MapPost("create", Create);
        routeGroupBuilder.MapGet("list", SMSList);
        routeGroupBuilder.MapPatch("update", UpdateSMS);
        routeGroupBuilder.MapPost("delete", DeleteSMS);
    }

    private static async Task<IResult> Create(
        [FromBody][Required] CreateSMSCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(command, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateSMSResponse>(createResponse));
    }

    private static async Task<IResult> SMSList(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetSMSListQuery(search, sort, order, page, size),
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetSMSListResponse>(list));
    }

    public static async Task<IResult> UpdateSMS(
        [FromBody][Required] UpdateSMSCommand updateCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> DeleteSMS(
        [FromBody][Required] DeleteSMSCommand deleteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(deleteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }
}
