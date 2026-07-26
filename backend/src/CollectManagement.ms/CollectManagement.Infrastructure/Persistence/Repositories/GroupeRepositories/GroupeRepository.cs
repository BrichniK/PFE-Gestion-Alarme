using System.ComponentModel;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.GroupeRepositories;

public class GroupeRepository : RepositoryBase<Groupe>, IGroupeRepository
{
    public GroupeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Groupe>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.Nom.Contains(search)
        );

        var orderBy = where
            .OrderByDescending(o => o.Nom);

        var prop = TypeDescriptor
            .GetProperties(typeof(Groupe))
            .Find(sort ?? "Nom", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Groupe>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Groupe>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => Groupe.Create(
                c.GroupeId,
                c.Nom,
                c.Color,
                c.EmployeeIds
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Groupe> GetOneAsync(GroupeId groupeId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.GroupeId.Equals(groupeId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Groupe groupe, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.GroupeId.Equals(groupe.GroupeId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Nom, groupe.Nom)
                    .SetProperty(p => p.Color, groupe.Color)
                    .SetProperty(p => p.EmployeeIds, groupe.EmployeeIds),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
