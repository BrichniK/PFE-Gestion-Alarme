using CollectManagement.Domain.Types.ValueObjects;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Application.Interfaces.Repositories.Types;

public interface ITypeRepository : IRepositoryBase<Type>
{
    Task<(IReadOnlyList<Type>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Type> GetOneAsync(
        TypeId typeId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Type type, CancellationToken cancellationToken);

    Task<Type?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
