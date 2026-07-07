using System.Net;
using System.Net.Mail;
using CollectManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromAddress;
    private readonly string _fromDisplayName;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = configuration.GetValue<int?>("Email:SmtpPort") ?? 587;
        _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
        _fromAddress = configuration["Email:FromAddress"] ?? _smtpUsername;
        _fromDisplayName = configuration["Email:FromDisplayName"] ?? "Gestion d'Alerte";
        _enableSsl = configuration.GetValue<bool?>("Email:EnableSsl") ?? true;
    }

    public async Task SendEmailAsync(List<string> toAddresses, string subject, string body, CancellationToken cancellationToken)
    {
        if (!toAddresses.Any())
        {
            _logger.LogInformation("No email recipients provided, skipping email send");
            return;
        }

        try
        {
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress, _fromDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };

            foreach (var address in toAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct())
            {
                mailMessage.To.Add(address);
            }

            if (mailMessage.To.Count == 0)
            {
                _logger.LogInformation("No valid email addresses after filtering, skipping email send");
                return;
            }

            _logger.LogInformation("Sending email to {EmailAddresses} with subject: {Subject}",
                string.Join(", ", mailMessage.To.Select(t => t.Address)), subject);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {EmailAddresses}",
                string.Join(", ", mailMessage.To.Select(t => t.Address)));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Email request timed out/canceled for {EmailAddresses}",
                string.Join(", ", toAddresses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {EmailAddresses}",
                string.Join(", ", toAddresses));
        }
    }
}
