using System.Net;

namespace CollectManagement.Application.Common;

public class ApiResponse<TData>
{
    public ApiResponse()
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
    }
    
    public ApiResponse(string message)
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
        Message = message;
    }

    public ApiResponse(string message, bool success, int statusCode)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }
    
    public ApiResponse(TData data, string message = null)
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
        Message = message;
        Data = data;
    }

    public bool Success { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public List<string>? ValidationErrors { get; set; }
    public TData? Data { get; set; }
}