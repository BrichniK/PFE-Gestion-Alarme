using System.ComponentModel.DataAnnotations;
using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.Employees.Commands.CreateEmployee;
using CollectManagement.Application.Features.Employees.Commands.DeleteEmployee;
using CollectManagement.Application.Features.Employees.Commands.UpdateEmployee;
using CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;
using CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public class EmployeeEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("cm/employee").RequireAuthorization();

        routeGroupBuilder.MapGet("list", EmployeeList);
        routeGroupBuilder.MapPost("add", CreateEmployee);
        routeGroupBuilder.MapPatch("update", UpdateEmployee);
        routeGroupBuilder.MapPost("{id}/delete", DeleteEmployee);
        routeGroupBuilder.MapGet("{id}/one", OneEmployee);
    }

    public static async Task<IResult> CreateEmployee(
        [FromBody][Required] CreateEmployeeCommand createEmployeeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var createResponse = await sender
            .Send(createEmployeeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<CreateEmployeeResponse>(createResponse));
    }

    private static async Task<IResult> EmployeeList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int page,
        int size,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var list = await sender
            .Send(new GetPagedListEmployeeQuery(search, sort, order, page, size), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetPagedListEmployeeResponse>(list));
    }

    private static async Task<IResult> UpdateEmployee(
        [FromBody][Required] UpdateEmployeeCommand updateEmployeeCommand,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(updateEmployeeCommand, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    private static async Task<IResult> DeleteEmployee(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender
            .Send(new DeleteEmployeeCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<bool>(true));
    }

    public static async Task<IResult> OneEmployee(
        [Required] Ulid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender
            .Send(new GetOneEmployeeQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new ApiResponse<GetOneEmployeeResponse>(response));
    }
}
