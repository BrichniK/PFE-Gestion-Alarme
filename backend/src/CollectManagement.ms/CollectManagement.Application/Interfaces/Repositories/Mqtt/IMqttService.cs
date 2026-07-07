namespace CollectManagement.Application.Interfaces.Repositories.Mqtt;

public interface IMqttService
{
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    Task PublishAsync(string topic, object payload);
}
