using CollectManagement.Application.Interfaces.Repositories.Devices;

namespace CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;

public class GetPagedListDeviceQueryHandler
    : IRequestHandler<GetPagedListDeviceQuery, GetPagedListDeviceResponse>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public GetPagedListDeviceQueryHandler(IDeviceRepository deviceRepository, IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListDeviceResponse> Handle(GetPagedListDeviceQuery request, CancellationToken cancellationToken)
    {
        var (listDevice, length) = await _deviceRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListDeviceResponse(
            _mapper.Map<List<GetPagedListDeviceDto>>(listDevice),
            length
        );
    }
}
