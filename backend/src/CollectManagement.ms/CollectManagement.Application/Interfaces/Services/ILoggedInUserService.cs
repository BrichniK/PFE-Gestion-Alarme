namespace CollectManagement.Application.Interfaces.Services;

public interface ILoggedInUserService
{
    bool IsAuthenticated { get; }
    string UserId { get;  }
}