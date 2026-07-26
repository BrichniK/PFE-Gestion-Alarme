namespace CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;

public record GetOneEmployeeQuery(Ulid EmployeeId) : IRequest<GetOneEmployeeResponse>;
