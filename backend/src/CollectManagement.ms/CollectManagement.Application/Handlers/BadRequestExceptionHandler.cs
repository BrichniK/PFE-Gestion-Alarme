using System.Net;
using CollectManagement.Application.Common;
using CollectManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CollectManagement.Application.Handlers;

public class BadRequestExceptionHandler: IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException)
            return false;
        
        var exceptionMessage = exception.Message;

        await httpContext.Response.WriteAsJsonAsync(
                new ApiResponse<string>()
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exceptionMessage,
                    Data = "",
                    ValidationErrors = []
                }, cancellationToken)
            .ConfigureAwait(false);
        
        return true;
    }
}