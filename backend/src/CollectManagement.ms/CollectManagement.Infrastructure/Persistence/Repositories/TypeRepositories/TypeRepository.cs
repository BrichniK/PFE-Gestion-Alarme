using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Infrastructure.Persistence.Repositories.TypeRepositories;

public class TypeRepository : RepositoryBase<Type>, ITypeRepository
{
    public TypeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Type>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.Code.Contains(search) ||
            w.Label.Contains(search)
        );

        var orderBy = where
            .OrderByDescending(o => o.Code);

        var prop = TypeDescriptor
            .GetProperties(typeof(Type))
            .Find(sort ?? "Code", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Type>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Type>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => Type.Create(
                c.TypeId,
                c.Code,
                c.Label,
                c.DureeNominal
            ))
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Type> GetOneAsync(TypeId typeId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.TypeId.Equals(typeId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Type type, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.TypeId.Equals(type.TypeId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Code, type.Code)
                    .SetProperty(p => p.Label, type.Label)
                    .SetProperty(p => p.DureeNominal, type.DureeNominal),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Type?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
