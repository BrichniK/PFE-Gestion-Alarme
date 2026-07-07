namespace CollectManagement.Application.Features.Devices.Commands.DeleteDevice;

public record DeleteDeviceCommand(Ulid DeviceId) : IRequest;
