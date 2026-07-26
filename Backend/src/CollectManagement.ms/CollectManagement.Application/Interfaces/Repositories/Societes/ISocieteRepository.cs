using CollectManagement.Application.Interfaces.Repositories;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Interfaces.Societes;

public interface ISocieteRepository : IRepositoryBase<Societe>
{
    Task<(IReadOnlyList<Societe>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );
    Task <Societe> GetOneAsync (
        SocieteId societeId ,
        CancellationToken cancellationToken );
    
    Task UpdateBulkAsync(Societe societe, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an societe by their matriculeFiscal tag identifier.
    /// </summary>
    Task<Societe?> GetByMFAsync(string matriculeFiscal, CancellationToken cancellationToken);
}