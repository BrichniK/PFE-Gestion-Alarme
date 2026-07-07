namespace CollectManagement.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(List<string> toAddresses, string subject, string body, CancellationToken cancellationToken);
}
