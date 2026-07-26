using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

#pragma warning disable CA1848
        _logger.LogInformation("----> Handling {RequestName} : @{Request}", 
            requestName, request);
#pragma warning restore CA1848

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();
        
        timer.Stop();

        var timeTaken = timer.Elapsed.TotalSeconds;
        
#pragma warning disable CA1848
        _logger.LogInformation("----> Request {RequestName} handled ({TimeTaken} seconds)", 
            requestName, timeTaken);
#pragma warning restore CA1848

        return response;
    }
}