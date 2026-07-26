using CollectManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace CollectManagement.Infrastructure.Services;

public class MqttPublisher : IMqttPublisher
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttPublisher> _logger;

    public MqttPublisher(IMqttClient mqttClient, ILogger<MqttPublisher> logger)
    {
        _mqttClient = mqttClient;
        _logger = logger;
    }

    public async Task PublishAsync(string topic, object payload)
    {
        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("MQTT client not connected. Cannot publish to {Topic}", topic);
            return;
        }

        string payloadToSend = payload is string str ? str : JsonConvert.SerializeObject(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payloadToSend)
            .WithContentType("application/json")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
        _logger.LogInformation("Published message to topic: {Topic}", topic);
    }
}
