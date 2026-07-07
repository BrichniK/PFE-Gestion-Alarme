using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Application.Handlers;

public class ExceptionLoggingHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionLoggingHandler> _logger;

    public ExceptionLoggingHandler(ILogger<ExceptionLoggingHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var exceptionMessage = exception.Message;

#pragma warning disable CA1848
        _logger.LogError(
            "Exception with TraceId {TraceId} failed with message: {ExceptionMessage}",
            httpContext.TraceIdentifier, exceptionMessage);
#pragma warning restore CA1848
        
        return ValueTask.FromResult(false);
    }
}