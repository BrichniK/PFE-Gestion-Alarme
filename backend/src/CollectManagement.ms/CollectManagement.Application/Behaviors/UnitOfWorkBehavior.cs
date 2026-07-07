using System.Diagnostics.CodeAnalysis;
using CollectManagement.Application.Shared;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Application.Behaviors;

[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    :IPipelineBehavior<TRequest, TResponse>
    where TRequest: notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(
        IUnitOfWork unitOfWork, 
        ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("----> Start Unit Of Work");
        
        if (IsNotCommand())
        {
            _logger.LogInformation("----> Exit Unit Of Work, Request Is not a command");
            return await next().ConfigureAwait(false);
        }

        using var transaction = _unitOfWork.BeginTransactionAsync(cancellationToken);
        
           _logger.LogInformation("----> Unit Of Work start Transaction");
            //using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
         
            var response = await next().ConfigureAwait(false);
        
            await _unitOfWork
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
            
            //transactionScope.Complete();
            await _unitOfWork.CommitTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
            
            _logger.LogInformation("----> Unit Of Work Transaction Complete");
            
            return response;
       
    }

    private static bool IsNotCommand()
    {
        return !typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal);
    }
}