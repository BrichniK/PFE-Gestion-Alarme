namespace CollectManagement.Application.Interfaces.Services;

public interface ISMSService
{
    Task SendSMSAsync(List<string> phoneNumbers, string message, CancellationToken cancellationToken);

    Task SendSMSAsync(List<string> phoneNumbers, string message, string apiUrl, CancellationToken cancellationToken);
}
