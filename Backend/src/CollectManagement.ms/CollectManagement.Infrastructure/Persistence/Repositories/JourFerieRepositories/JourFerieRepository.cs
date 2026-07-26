using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.JourFerieRepositories;

public class JourFerieRepository : RepositoryBase<JourFerie>, IJourFerieRepository
{
    public JourFerieRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<JourFerie>, int)> GetPagedListAsync(
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
            .OrderByDescending(o => o.Date);

        var prop = TypeDescriptor
            .GetProperties(typeof(JourFerie))
            .Find(sort ?? "Date", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<JourFerie>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<JourFerie>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => JourFerie.Create(
                c.JourFerieId,
                c.Date,
                c.Label
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<JourFerie> GetOneAsync(JourFerieId jourFerieId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.JourFerieId.Equals(jourFerieId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(JourFerie jourFerie, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.JourFerieId.Equals(jourFerie.JourFerieId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Date, jourFerie.Date)
                    .SetProperty(p => p.Label, jourFerie.Label),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
