using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.UtilisateurRepositories;

public class RoleUtilisateurRepository : RepositoryBase<RoleUtilisateur>, IRoleUtilisateurRepository
{
    public Task<RoleUtilisateur?> GetOneAsync(RoleUtilisateurId roleUtilisateurId, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(w=>w.RoleUtilisateurId.Equals(roleUtilisateurId), cancellationToken);
    }
    
    public async Task<(List<RoleUtilisateur>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrEmpty(search) ||
            w.LibelleRoleUtilisateur.Contains(search));
        
        var orderBy = where
            .OrderByDescending(o => o.LibelleRoleUtilisateur);

        var prop = TypeDescriptor
            .GetProperties(typeof(RoleUtilisateur))
            .Find(sort??"LibelleRoleUtilisateur", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o => 
                EF.Property<RoleUtilisateur>(o, prop.DisplayName));
        
        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o => 
                EF.Property<RoleUtilisateur>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        
        var readOnlyList = await orderBy
            .Skip(page * size)
            .Take(size)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        return ( readOnlyList, countAsync);
    }
    
    public RoleUtilisateurRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}