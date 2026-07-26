using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Devices.Queries.GetOneDevice;

public class GetOneDeviceQueryHandler
    : IRequestHandler<GetOneDeviceQuery, GetOneDeviceResponse>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public GetOneDeviceQueryHandler(IDeviceRepository deviceRepository, IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<GetOneDeviceResponse> Handle(GetOneDeviceQuery request, CancellationToken cancellationToken)
    {
        var deviceId = new DeviceId(request.DeviceId);

        var device = await _deviceRepository
            .GetOneAsync(deviceId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Device NotFound.");

        return _mapper.Map<GetOneDeviceResponse>(device);
    }
}
