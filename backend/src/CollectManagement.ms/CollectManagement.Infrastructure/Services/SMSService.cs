using System.Text;
using System.Text.Json;
using CollectManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CollectManagement.Infrastructure.Services;

public class SMSService : ISMSService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SMSService> _logger;
    private readonly string _smsApiUrl;

    public SMSService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SMSService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _smsApiUrl = configuration["SMS:ApiUrl"] ?? "http://192.168.1.22/api/sms";
    }

    public Task SendSMSAsync(List<string> phoneNumbers, string message, CancellationToken cancellationToken)
    {
        return SendSMSAsync(phoneNumbers, message, _smsApiUrl, cancellationToken);
    }

    public async Task SendSMSAsync(List<string> phoneNumbers, string message, string apiUrl, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                numbers = phoneNumbers,
                message = message
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending SMS to {PhoneNumbers} with message: {Message}", 
                string.Join(", ", phoneNumbers), message);

            var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumbers}", string.Join(", ", phoneNumbers));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send SMS. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (TaskCanceledException ex)
        {
            // Timeout or cancellation — don't fail MQTT pipeline
            _logger.LogError(ex, "SMS request timed out/canceled for {PhoneNumbers}", string.Join(", ", phoneNumbers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumbers}", string.Join(", ", phoneNumbers));
        }
    }
}
