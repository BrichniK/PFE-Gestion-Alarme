using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess.ObjectValues;


namespace CollectManagement.Application.Features.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
{

    private readonly IEmployeeRepository _employeeRepository;

    public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(request.EmployeeId);
        await _employeeRepository
            .DeleteAsync(
                w => w.EmployeeId.Equals(employeeId), cancellationToken)
            .ConfigureAwait(false);
    }

}