using System.ComponentModel;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.EmployeeRepositories;

public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Employee>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.Nom.Contains(search) ||
            w.Prenom.Contains(search) ||
            w.Rfid.Contains(search)
        );

        var orderBy = where
            .OrderByDescending(o => o.Nom);

        var prop = TypeDescriptor
            .GetProperties(typeof(Employee))
            .Find(sort ?? "Nom", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Employee>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Employee>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => Employee.Create(
                c.EmployeeId,
                c.Nom,
                c.Prenom,
                c.Phone,
                c.Rfid,
                c.Email,
                c.LogoPath
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Employee> GetOneAsync(EmployeeId employeeId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.EmployeeId.Equals(employeeId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Employee employee, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.EmployeeId.Equals(employee.EmployeeId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Nom, employee.Nom)
                    .SetProperty(p => p.Prenom, employee.Prenom)
                    .SetProperty(p => p.Phone, employee.Phone)
                    .SetProperty(p => p.Rfid, employee.Rfid)
                    .SetProperty(p => p.Email, employee.Email)
                    .SetProperty(p => p.LogoPath, employee.LogoPath),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Employee?> GetByRfidAsync(string rfid, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(w => w.Rfid == rfid)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
