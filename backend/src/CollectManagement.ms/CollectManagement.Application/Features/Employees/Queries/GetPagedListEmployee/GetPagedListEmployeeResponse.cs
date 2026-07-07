namespace CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;

public record GetPagedListEmployeeResponse(
    List<GetPagedListEmployeeDto> Employees,
    int Length
);
