using System.Linq.Expressions;
using CollectManagement.Application.Interfaces.Repositories;
using CollectManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Query;

namespace CollectManagement.Infrastructure.Repositories;

public class RepositoryBase<TEntity>
:IRepositoryBase<TEntity>
where TEntity : class
{
   
    private readonly ApplicationDbContext _dbContext;
    protected DbSet<TEntity> _dbSet => _dbContext.Set<TEntity>();
    
    public RepositoryBase(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void Attach(TEntity entity)
    {
        _dbSet.Attach(entity);
    }
    
    public void AttachRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AttachRange(entities);
    }
    
    public void Detach(TEntity entity)
    {
        _dbSet.Entry(entity).State = EntityState.Detached;
    }

    public async ValueTask<TEntity> AddAsync(
        TEntity entity, 
        CancellationToken cancellationToken)
    {
        var valueTask = await _dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
        return valueTask.Entity;
    }
    
    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities, 
        CancellationToken cancellationToken)
    {
        await _dbSet
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }

    public TEntity Update(TEntity entity)
    {
        return _dbSet.Update(entity).Entity;
    }
    
    public void UpdateState(TEntity entity)
    {
        _dbSet.Entry(entity).State = EntityState.Modified;
    }
    
    public void DeleteState(TEntity entity)
    {
        _dbSet.Entry(entity).State = EntityState.Deleted;
    }

    public Task<int> BulkUpdateAsync<TExpression>(Expression predicate, TExpression properties, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// this method update entity using bulk update with where expresion and
    /// property specification
    /// </summary>
    /// <param name="predicate">Condition to use for the update</param>
    /// <param name="properties">property to be updated</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TExpression">Expression&lt;Func&lt;SetPropertyCalls&lt;TEntity&gt;, SetPropertyCalls&lt;TEntity&gt;&gt;&gt;</typeparam>
    /// <returns></returns>
    public Task<int> BulkUpdateAsync<TExpression>(Expression<Func<TEntity, bool>> predicate,
        TExpression properties,
        CancellationToken cancellationToken)
    {
        var props = properties as Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>;
        return _dbSet.Where(predicate).ExecuteUpdateAsync(props!, cancellationToken);
    }
    
    public Task<int> BulkUpdateAsync<TExpression>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate).ExecuteUpdateAsync(null,cancellationToken);
    }

    public int Delete(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate).ExecuteDelete();
    }

    public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }

    public TEntity? GetById<TId>(TId id)
    {
        return _dbSet.Find(id);
    }

    public ValueTask<TEntity?> GetByIdAsync<TId>(TId Id)
    {
        return _dbSet.FindAsync(Id);
    }

    public TEntity? Get(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.FirstOrDefault(predicate);
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate
        , CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate:predicate).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public IEnumerable<TEntity> GetAll()
    {
        return _dbSet.ToList();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync().ConfigureAwait(false);
    }    
    
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public IEnumerable<TEntity>? GetMany(Expression<Func<TEntity, bool>> predicate = null)
    {
        return _dbSet.Where(predicate).ToList();
    }

    public Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }
    
    public async virtual Task<IReadOnlyList<TEntity>> GetPagedReponseAsync(
        int page, 
        int size, 
        CancellationToken cancellationToken)
    {
        return await _dbSet.Skip((page - 1) * size)
            .Take(size)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual Task<List<TResult>> ListSelect<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
        => _dbSet.Select(selector).ToListAsync(cancellationToken);
    public virtual Task<List<TResult>> ListSelectMany<TResult>(Expression<Func<TEntity, IEnumerable<TResult>>> selector, CancellationToken cancellationToken)
        => _dbSet.SelectMany(selector).ToListAsync(cancellationToken);

    public Task<TResult> SqlQueryFirstAsync<TResult>(FormattableString sql)
        => _dbContext.Database.SqlQuery<TResult>(sql).FirstAsync();
    
    public Task<TResult?> SqlQueryFirstOrDefaultAsync<TResult>(FormattableString sql)
        => _dbContext.Database.SqlQuery<TResult>(sql).FirstOrDefaultAsync();
    
    public Task<List<TResult>> SqlQueryListAsync<TResult>(FormattableString sql)
        => _dbContext.Database.SqlQuery<TResult>(sql).ToListAsync();}
