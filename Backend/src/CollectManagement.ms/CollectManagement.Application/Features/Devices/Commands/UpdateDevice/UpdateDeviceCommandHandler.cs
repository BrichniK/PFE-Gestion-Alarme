using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Devices.Commands.UpdateDevice;

public class UpdateDeviceCommandHandler
    : IRequestHandler<UpdateDeviceCommand>
{
    private readonly IDeviceRepository _deviceRepository;

    public UpdateDeviceCommandHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var deviceId = new DeviceId(request.DeviceId);

        var device = Device.Create(
            deviceId,
            request.DeviceName,
            request.Matricule,
            request.NombreCapteur
        );

        await _deviceRepository.UpdateBulkAsync(device, cancellationToken)
            .ConfigureAwait(false);
    }
}
