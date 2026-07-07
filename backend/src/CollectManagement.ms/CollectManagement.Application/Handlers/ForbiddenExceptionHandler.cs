using System.Net;
using CollectManagement.Application.Common;
using CollectManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CollectManagement.Application.Handlers;

public class ForbiddenExceptionHandler: IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException)
            return false;
        
        var exceptionMessage = exception.Message;

        await httpContext.Response.WriteAsJsonAsync(
                new ApiResponse<string>()
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = exceptionMessage,
                    Data = "",
                    ValidationErrors = []
                }, cancellationToken)
            .ConfigureAwait(false);
        
        return true;
    }
}