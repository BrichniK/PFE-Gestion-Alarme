using System.Net;
using CollectManagement.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CollectManagement.Application.Handlers;

public class GlobalExceptionHandling : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var exceptionMessage = exception.Message;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiResponse<string>()
        {
            Success = false,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = exceptionMessage,
            Data = "",
            ValidationErrors = []
        }, cancellationToken)
            .ConfigureAwait(false);
        
        return true;
    }
}