using System.Data;
using CollectManagement.Application.Interfaces.Repositories;

namespace CollectManagement.Application.Shared;

public interface IUnitOfWork:IDisposable
{
    void SaveChanges();

    Task SaveChangesAsync(CancellationToken cancellationToken);
    
    IRepositoryBase<T> GetRepository<T>() where T : class;

    IDbTransaction BeginTransaction();
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
}