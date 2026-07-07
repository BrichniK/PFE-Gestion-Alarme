using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Devices.Commands.DeleteDevice;

public class DeleteDeviceCommandHandler
    : IRequestHandler<DeleteDeviceCommand>
{
    private readonly IDeviceRepository _deviceRepository;

    public DeleteDeviceCommandHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var deviceId = new DeviceId(request.DeviceId);

        await _deviceRepository
            .DeleteAsync(
                w => w.DeviceId.Equals(deviceId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
