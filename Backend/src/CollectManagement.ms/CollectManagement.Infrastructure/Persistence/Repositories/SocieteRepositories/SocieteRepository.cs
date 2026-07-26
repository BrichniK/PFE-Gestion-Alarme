using System.ComponentModel;
using CollectManagement.Application.Interfaces.Societes;

using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Repositories.SocieteRepositories;

public class SocieteRepository : RepositoryBase<Societe>, ISocieteRepository
{
    public SocieteRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Societe>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w=>
            string.IsNullOrWhiteSpace(search) ||
            w.Nom.Contains(search) ||
            w.Email.Contains(search) ||
            w.Fax1.Contains(search) ||
            w.Fax2.Contains(search) ||
            w.Adresse.Contains(search) ||
            w.MatriculeFiscal.Contains(search) ||
            w.Adresse.Contains(search) ||
            w.Telephone2.Contains(search) ||
            w.Telephone1.Contains(search)
        );
        
        var orderBy = where
            .OrderByDescending(o => o.Nom);

        var prop = TypeDescriptor
            .GetProperties(typeof(Societe))
            .Find(sort??"Nom", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o => 
                EF.Property<Societe>(o, prop.DisplayName));
        
        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o => 
                EF.Property<Societe>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        
        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c=> Societe.QueryCreate(
                c.SocieteId,
                c.LogoPath,
                c.Nom,
                c.MatriculeFiscal,
                c.Rne,
                c.Capital,
                c.DateOverture,
                c.Telephone1,
                c.Telephone2,
                c.Fax1,
                c.Fax2,
                c.Email,
                c.Adresse,
                c.CodeSociete
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        return ( readOnlyList, countAsync);
        
    }

    public Task<Societe> GetOneAsync(SocieteId societeId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w=>w.SocieteId.Equals(societeId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async  Task UpdateBulkAsync(Societe societe, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.SocieteId.Equals(societe.SocieteId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p=>p.Nom,societe.Nom)
                    .SetProperty(p=>p.LogoPath,societe.LogoPath)
                    .SetProperty(p=>p.MatriculeFiscal,societe.MatriculeFiscal)
                    .SetProperty(p=>p.Rne,societe.Rne)
                    .SetProperty(p=>p.Capital,societe.Capital)
                    .SetProperty(p=>p.DateOverture,societe.DateOverture)
                    .SetProperty(p=>p.Telephone1,societe.Telephone1)
                    .SetProperty(p=>p.Telephone2,societe.Telephone2)
                    .SetProperty(p=>p.Fax1,societe.Fax1)
                    .SetProperty(p=>p.Fax2,societe.Fax2)
                    .SetProperty(p=>p.Email,societe.Email)
                    .SetProperty(p=>p.Adresse,societe.Adresse)
                    .SetProperty(p=>p.CodeSociete,societe.CodeSociete)
                   
                , cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Societe?> GetByMFAsync(string matriculeFiscal, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}