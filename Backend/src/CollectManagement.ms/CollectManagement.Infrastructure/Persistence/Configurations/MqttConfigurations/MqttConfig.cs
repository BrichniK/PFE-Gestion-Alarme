namespace CollectManagement.Infrastructure.Persistence.Configurations.MqttConfigurations;

public sealed class MqttConfig
{
    public required string Broker { get; init; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? Topic { get; set; }
}
