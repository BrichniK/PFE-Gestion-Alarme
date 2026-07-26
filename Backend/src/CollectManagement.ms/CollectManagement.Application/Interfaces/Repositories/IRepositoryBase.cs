using System.Linq.Expressions;

namespace CollectManagement.Application.Interfaces.Repositories;

public interface IRepositoryBase<TEntity>
    where TEntity : class
{
    void Attach(TEntity entity);
    void AttachRange(IEnumerable<TEntity> entities);
    void Detach(TEntity entity);
    ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    TEntity Update(TEntity entity);
    void UpdateState(TEntity entity);
    void DeleteState(TEntity entity);
    
    Task<int> BulkUpdateAsync<TExpression>(Expression<Func<TEntity, bool>> predicate, 
        TExpression properties,
        CancellationToken cancellationToken);
    int Delete(Expression<Func<TEntity, bool>> predicate);
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    TEntity? GetById<TId>(TId Id);
    ValueTask<TEntity?> GetByIdAsync<TId>(TId Id);
    TEntity? Get(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    IEnumerable<TEntity> GetAll();
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    IEnumerable<TEntity>? GetMany(Expression<Func<TEntity, bool>> predicate = null);
    Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IReadOnlyList<TEntity>> GetPagedReponseAsync(int page, int size, CancellationToken cancellationToken);
    Task<List<TResult>> ListSelect<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken);
    Task<List<TResult>> ListSelectMany<TResult>(Expression<Func<TEntity, IEnumerable<TResult>>> selector, CancellationToken cancellationToken);
    Task<TResult> SqlQueryFirstAsync<TResult>(FormattableString sql);
    Task<TResult?> SqlQueryFirstOrDefaultAsync<TResult>(FormattableString sql);
    Task<List<TResult>> SqlQueryListAsync<TResult>(FormattableString sql);
}