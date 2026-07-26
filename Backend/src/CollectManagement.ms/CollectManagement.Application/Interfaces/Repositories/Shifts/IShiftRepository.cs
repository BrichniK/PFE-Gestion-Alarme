using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Shifts;

public interface IShiftRepository : IRepositoryBase<Shift>
{
    Task<(IReadOnlyList<Shift>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Shift> GetOneAsync(
        ShiftId shiftId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Shift shift, CancellationToken cancellationToken);
}
