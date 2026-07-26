using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Devices.Commands.CreateDevice;

public class CreateDeviceCommandHandler
    : IRequestHandler<CreateDeviceCommand, CreateDeviceResponse>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public CreateDeviceCommandHandler(
        IDeviceRepository deviceRepository,
        IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<CreateDeviceResponse> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var deviceId = new DeviceId(Ulid.NewUlid());

        var device = Device.Create(
            deviceId,
            request.DeviceName,
            request.Matricule,
            request.NombreCapteur
        );

        await _deviceRepository
            .AddAsync(device, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateDeviceResponse>(device);
    }
}
