using System.Diagnostics.CodeAnalysis;
using CollectManagement.Application.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IValidator<TRequest>? _validator;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        ILogger<ValidationBehavior<TRequest, TResponse>> logger, 
        IValidator<TRequest>? validator = null)
    {
        _logger = logger;
        _validator = validator;
    }

    [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("----> Start Validating");
        
        if (_validator is null)
        {
            _logger.LogInformation("----> No Validator presented");
            return await next();
        }
        
        
        var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            throw new CustomValidationException(validationResult);
        }
        
        _logger.LogInformation("----> Validator Complete");
        
        return await next();
    }
}