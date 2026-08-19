using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;

namespace CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;

public class GetPagedListSensorMeasurementQueryHandler
    : IRequestHandler<
        GetPagedListSensorMeasurementQuery,
        GetPagedListSensorMeasurementResponse>
{
    private readonly ISensorMeasurementRepository _sensorMeasurementRepository;
    private readonly IMapper _mapper;

    public GetPagedListSensorMeasurementQueryHandler(
        ISensorMeasurementRepository sensorMeasurementRepository,
        IMapper mapper)
    {
        _sensorMeasurementRepository = sensorMeasurementRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListSensorMeasurementResponse> Handle(
        GetPagedListSensorMeasurementQuery request,
        CancellationToken cancellationToken)
    {
        var (measurements, count) =
            await _sensorMeasurementRepository.GetPagedListAsync(
                    request.DeviceId,
                    request.SensorCode,
                    request.From,
                    request.To,
                    request.Page,
                    request.Size,
                    cancellationToken)
                .ConfigureAwait(false);

        var items = _mapper
            .Map<IReadOnlyList<GetPagedListSensorMeasurementDto>>(measurements);

        return new GetPagedListSensorMeasurementResponse(
            items,
            count);
    }
}