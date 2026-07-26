using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.JoursFeries;

public interface IJourFerieRepository : IRepositoryBase<JourFerie>
{
    Task<(IReadOnlyList<JourFerie>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<JourFerie> GetOneAsync(
        JourFerieId jourFerieId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(JourFerie jourFerie, CancellationToken cancellationToken);
}
