using CollectManagement.Application.Interfaces.Repositories;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;


namespace CollectManagement.Application.Interfaces.Groupes;

public interface IGroupeRepository : IRepositoryBase<Groupe>
{
    Task<(IReadOnlyList<Groupe>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Groupe> GetOneAsync(
        GroupeId groupeId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Groupe groupe, CancellationToken cancellationToken);
}