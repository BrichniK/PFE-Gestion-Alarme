namespace CollectManagement.Application.Interfaces.Services;

public interface IMqttPublisher
{
    Task PublishAsync(string topic, object payload);
}
