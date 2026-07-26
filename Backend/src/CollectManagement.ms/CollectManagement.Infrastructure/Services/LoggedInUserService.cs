using System.Security.Claims;
using CollectManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace CollectManagement.Infrastructure.Services;

public class LoggedInUserService : ILoggedInUserService
{
    private readonly IHttpContextAccessor _context;
    
    public LoggedInUserService(IHttpContextAccessor httpContextAccessor)
    {
        _context = httpContextAccessor;
    }
    
    public bool IsAuthenticated
    {
        get
        {
            return _context.HttpContext.User.Identity.IsAuthenticated;
        }
    }

    public string UserId
    {
        get
        {
            var userId = _context.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? _context.HttpContext?.User.FindFirst("sub")?.Value;

            return userId;
        }
    }

    public IReadOnlyList<KeyValuePair<string, string>>? Claims
    {
        get
        {
            return _context.HttpContext?.User.Claims.ToList()
                .ConvertAll(item =>
                    new KeyValuePair<string, string>(
                        item.Type,
                        item.Value));
        }
    }
}