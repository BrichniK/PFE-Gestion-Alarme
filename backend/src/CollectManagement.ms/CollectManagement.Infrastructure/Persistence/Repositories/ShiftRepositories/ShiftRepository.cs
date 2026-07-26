using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.ShiftRepositories;

public class ShiftRepository : RepositoryBase<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Shift>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.Label.Contains(search)
        );

        var orderBy = where
            .OrderByDescending(o => o.Label);

        var prop = TypeDescriptor
            .GetProperties(typeof(Shift))
            .Find(sort ?? "Label", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Shift>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Shift>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => Shift.Create(
                c.ShiftId,
                c.Label,
                c.StartTime,
                c.EndTime
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Shift> GetOneAsync(ShiftId shiftId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.ShiftId.Equals(shiftId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Shift shift, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.ShiftId.Equals(shift.ShiftId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Label, shift.Label)
                    .SetProperty(p => p.StartTime, shift.StartTime)
                    .SetProperty(p => p.EndTime, shift.EndTime),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
