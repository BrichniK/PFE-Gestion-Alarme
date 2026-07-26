using MQTTnet;

namespace CollectManagement.Application.Interfaces.Services;

public interface IMqttMessageHandler
{
    Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e);
}
