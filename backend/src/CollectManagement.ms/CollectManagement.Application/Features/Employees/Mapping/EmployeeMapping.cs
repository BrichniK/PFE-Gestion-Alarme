using CollectManagement.Application.Features.Employees.Commands.CreateEmployee;
using CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;
using CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;
using CollectManagement.Domain.Employess;

namespace CollectManagement.Application.Features.Employees.Mapping;

public class EmployeeMapping
    : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, CreateEmployeeResponse>()
            .Map(d => d.EmployeeId, s => s.EmployeeId.Value);
        // manquant
        config.NewConfig<Employee, GetOneEmployeeResponse>()
            .Map(d => d.EmployeeId, s => s.EmployeeId.Value);

        config.NewConfig<Employee, GetPagedListEmployeeDto>()
            .Map(d => d.EmployeeId, s => s.EmployeeId.Value);

    }
}
