using System.Text;
using System.Text.Json;
using CollectManagement.Application.Interfaces.Repositories.Mqtt;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure.Persistence.Configurations.MqttConfigurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Protocol;

namespace CollectManagement.Infrastructure.Services;

public class MqttService : IMqttService, IAsyncDisposable
{
    private readonly ILogger<MqttService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttConfig _mqttConfig;
    private readonly IMqttClient _client;
    private readonly SemaphoreSlim _reconnectLock = new(1, 1);
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isDisposing;

    public bool IsConnected => _client?.IsConnected ?? false;

    public MqttService(
        ILogger<MqttService> logger,
        IOptions<MqttConfig> mqttConfig,
        IServiceScopeFactory scopeFactory,
        IMqttClient client)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _mqttConfig = mqttConfig.Value;
        _client = client;

        RegisterMessageHandler();
        RegisterDisconnectedHandler();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected) return;

        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsConnected) return;

            _logger.LogInformation("Connecting to MQTT broker at {Broker}:{Port}...", _mqttConfig.Broker, _mqttConfig.Port);

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttConfig.Broker, _mqttConfig.Port)
                .WithClientId(_mqttConfig.ClientId)
                .WithCredentials(_mqttConfig.Username, _mqttConfig.Password)
                .Build();

            try
            {
                await _client.ConnectAsync(options, cancellationToken);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("connect/disconnect is pending", StringComparison.OrdinalIgnoreCase))
            {
                // Another connect/disconnect is already in progress at the MQTT client level.
                // Treat this as benign and let the in-flight operation complete.
                _logger.LogDebug("Connect skipped because another connect/disconnect is pending.");
                return;
            }

            _logger.LogInformation("Connected to MQTT broker.");

            await SubscribeAsync(cancellationToken);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task SubscribeAsync(CancellationToken cancellationToken)
    {
        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(_mqttConfig.Topic)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _client.SubscribeAsync(topicFilter, cancellationToken);
        _logger.LogInformation("Subscribed to topic: {Topic}", _mqttConfig.Topic);
    }

    private void RegisterMessageHandler()
    {
        _client.ApplicationMessageReceivedAsync += async args =>
        {
            var payloadString = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            _logger.LogInformation("Message received on topic: {Topic}", args.ApplicationMessage.Topic);
            _logger.LogInformation("Message payload: {Payload}", payloadString);

            try
            {
                // Create a new scope per message so scoped services (DbContext, repos) are not long-lived
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMqttMessageHandler>();
                await handler.HandleMessageAsync(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MQTT message");
            }
        };
    }

    private void RegisterDisconnectedHandler()
    {
        _client.DisconnectedAsync += async e =>
        {
            _logger.LogWarning("MQTT client disconnected. Reason: {Reason}", e.Reason);
            if (!_isDisposing)
                await AttemptReconnectAsync();
        };
    }

    private async Task AttemptReconnectAsync()
    {
        await _reconnectLock.WaitAsync();
        try
        {
            int attempt = 0;
            const int maxAttempts = 5;
            var initialDelay = TimeSpan.FromSeconds(1);
            var maxDelay = TimeSpan.FromMinutes(5);

            while (!_client.IsConnected && attempt < maxAttempts)
            {
                try
                {
                    double delayMs = Math.Min(
                        initialDelay.TotalMilliseconds * Math.Pow(2, attempt),
                        maxDelay.TotalMilliseconds);
                    var delay = TimeSpan.FromMilliseconds(delayMs);

                    _logger.LogInformation("Reconnecting in {Delay}... (Attempt {Attempt})", delay, attempt + 1);
                    await Task.Delay(delay);
                    await ConnectAsync(CancellationToken.None);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reconnect attempt {Attempt} failed", attempt + 1);
                    attempt++;
                }
            }
        }
        finally
        {
            _reconnectLock.Release();
        }
    }

    public async Task PublishAsync(string topic, object payload)
    {
        if (!_client.IsConnected)
        {
            _logger.LogWarning("Cannot publish: MQTT client is not connected.");
            try
            {
                await ConnectAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection failed while trying to publish.");
                return;
            }
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(payload))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _client.PublishAsync(message);
        _logger.LogInformation("Published message to topic: {Topic}", topic);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _isDisposing = true;
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
            _logger.LogInformation("MQTT client disconnected gracefully.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposing = true;
        await DisconnectAsync();
        _client?.Dispose();
        _reconnectLock?.Dispose();
    }
}
