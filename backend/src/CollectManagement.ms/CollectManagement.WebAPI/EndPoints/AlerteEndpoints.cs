using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;
using CollectManagement.Application.Features.Alertes.Commands.DeleteAlerte;
using CollectManagement.Application.Features.Alertes.Commands.TraiterAlerte;
using CollectManagement.Application.Features.Alertes.Commands.UpdateAlerte;
using CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;
using CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;
using CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CollectManagement.WebAPI.EndPoints;

public class AlerteEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/alerte").RequireAuthorization();

        routeGroupBuilder.MapGet("list", AlerteList);
        routeGroupBuilder.MapPost("add", CreateAlerte);
        routeGroupBuilder.MapPatch("update", UpdateAlerte);
        routeGroupBuilder.MapPost("{id}/delete", DeleteAlerte);
        routeGroupBuilder.MapGet("{id}/one", OneAlerte);
        routeGroupBuilder.MapPost("traiter", TraiterAlerte);
        routeGroupBuilder.MapGet("employees-by-planning", GetEmployeesByPlanning);
    }

    public static async Task<IResult> CreateAlerte(
        [FromBody][Required] CreateAlerteCommand createAlerteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createAlerteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateAlerteResponse>(createResponse));
    }

    private static async Task<IResult> AlerteList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListAlerteQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListAlerteResponse>(list));
    }

    private static async Task<IResult> UpdateAlerte(
        [FromBody][Required] UpdateAlerteCommand updateAlerteCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateAlerteCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteAlerte(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteAlerteCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneAlerte(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneAlerteQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneAlerteResponse>(response));
    }

    private static IResult TraiterAlerte(
        [FromBody][Required] TraiterAlerteCommand command,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AlerteEndpoints> logger)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender
                    .Send(command, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process alert assignment for alert {AlerteId} and employee {EmployeeId}",
                    command.AlerteId,
                    command.EmployeeId
                );
            }
        });

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> GetEmployeesByPlanning(
        [FromQuery][Required] DateTime date,
        [FromQuery][Required] Ulid deviceId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupes = await sender
            .Send(new GetEmployeesByPlanningQuery(date, deviceId), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<List<GroupeWithEmployeesDto>>(groupes));
    }
}
