using MediatR;

namespace CollectManagement.Application.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Ulid EmployeeId) : IRequest;