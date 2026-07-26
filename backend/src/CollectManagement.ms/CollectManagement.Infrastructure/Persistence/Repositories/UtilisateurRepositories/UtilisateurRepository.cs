using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Enums;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.UtilisateurRepositories;

public class UtilisateurRepository : RepositoryBase<Utilisateur>, IUtilisateurRepository
{
      public async Task<(IReadOnlyList<Utilisateur>, int)> GetPagedListAsync(string? search, string? sort, string? order, int page, int size,
        CancellationToken cancellationToken)
    
    {

        var where = _dbSet.Where(w =>
            string.IsNullOrEmpty(search) ||
            w.NomUtilisateur.Contains(search) ||
            w.Nom.Contains(search) ||
            w.Email.Contains(search) ||
            w.Prenom.Contains(search))
            ;
        
        var orderBy = where
            .OrderByDescending(o => o.NomUtilisateur);

        var prop = TypeDescriptor
            .GetProperties(typeof(Utilisateur))
            .Find(sort??"NomUtilisateur", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o => 
                EF.Property<Utilisateur>(o, prop.DisplayName));
        
        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o => 
                EF.Property<Utilisateur>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        
        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c=> Utilisateur.QueryCreate(
                c.UtilisateurId,
                c.NomUtilisateur,
                c.Nom,
                c.Prenom,
                c.Email,
                c.Password,
                c.RoleUtilisateurId,
                c.IsActive,
                c.SocieteId
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        return ( readOnlyList, countAsync);
    }

    public Task<Utilisateur?> TryToLogin(
        string login, 
        CancellationToken cancellationToken)
    {
        return _dbSet.Where(u=>
                (u.Email==login || u.NomUtilisateur == login) &&
                u.IsActive
            )
            .Include(i=>i.RoleUtilisateur)
            .Include(i=>i.Societe)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Utilisateur?> GetOneAsync(UtilisateurId utilisateurId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w=>w.UtilisateurId.Equals(utilisateurId))
            .Include(i=>i.RoleUtilisateur)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public UtilisateurRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

}