namespace CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;

public record GetPagedListEmployeeQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListEmployeeResponse>;
