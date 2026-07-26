using System.Data;
using CollectManagement.Application.Interfaces.Repositories;
using CollectManagement.Application.Shared;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CollectManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    
    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);
    

    public IRepositoryBase<TEntity> GetRepository<TEntity>() where TEntity : class
        => new RepositoryBase<TEntity>(_dbContext);
    

    public IDbTransaction BeginTransaction()
        => _dbContext.Database.BeginTransaction().GetDbTransaction();

    public Task BeginTransactionAsync(CancellationToken cancellationToken)
        => _dbContext.Database.BeginTransactionAsync(cancellationToken);
    
    public Task CommitTransactionAsync(CancellationToken cancellationToken)
        => _dbContext.Database.CommitTransactionAsync(cancellationToken);
    
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }
}