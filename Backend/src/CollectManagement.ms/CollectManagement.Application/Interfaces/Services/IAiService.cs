namespace CollectManagement.Application.Interfaces.Services;

public interface IAiService
{
    Task<string> GenerateResponseAsync(
        string userMessage,
        string context,
        CancellationToken cancellationToken);
}