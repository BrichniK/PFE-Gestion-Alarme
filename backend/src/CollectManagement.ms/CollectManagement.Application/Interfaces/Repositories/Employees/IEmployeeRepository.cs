using CollectManagement.Application.Interfaces.Repositories;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;


namespace CollectManagement.Application.Interfaces.Employees;

public interface IEmployeeRepository : IRepositoryBase<Employee>
{
    Task<(IReadOnlyList<Employee>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Employee> GetOneAsync(
        EmployeeId employeeId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Employee employee, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an employee by their RFID tag identifier.
    /// </summary>
    Task<Employee?> GetByRfidAsync(string rfid, CancellationToken cancellationToken);
}