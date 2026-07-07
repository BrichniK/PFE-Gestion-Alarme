using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Utilisateurs;

public interface IRoleUtilisateurRepository : IRepositoryBase<RoleUtilisateur>
{
    Task<RoleUtilisateur?> GetOneAsync(RoleUtilisateurId roleUtilisateurId, CancellationToken cancellationToken);

    Task<(List<RoleUtilisateur>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken);
}