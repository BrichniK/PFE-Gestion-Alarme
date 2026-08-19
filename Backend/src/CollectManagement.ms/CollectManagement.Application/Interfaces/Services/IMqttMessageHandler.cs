using MQTTnet;
using Newtonsoft.Json;

namespace CollectManagement.Application.Interfaces.Services;

public interface IMqttMessageHandler
{
    Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e);
    
}

public class SensorMeasurementMqttMessage
{
    [JsonProperty("deviceId")]
    public string? DeviceId { get; set; }

    [JsonProperty("temperature")]
    public double? Temperature { get; set; }

    [JsonProperty("vibration")]
    public double? Vibration { get; set; }

    [JsonProperty("pressure")]
    public double? Pressure { get; set; }

    [JsonProperty("humidity")]
    public double? Humidity { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    public bool IsFailure =>
        string.Equals(Status, "FAILURE", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Status, "CRITICAL", StringComparison.OrdinalIgnoreCase);
}
