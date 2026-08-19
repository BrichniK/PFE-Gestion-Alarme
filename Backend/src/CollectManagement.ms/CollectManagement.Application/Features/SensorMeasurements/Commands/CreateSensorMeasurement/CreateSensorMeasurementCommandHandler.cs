using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;

namespace CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;

public class CreateSensorMeasurementCommandHandler
    : IRequestHandler<CreateSensorMeasurementCommand, CreateSensorMeasurementResponse>
{
    private readonly ISensorMeasurementRepository _sensorMeasurementRepository;
    private readonly IMapper _mapper;

    public CreateSensorMeasurementCommandHandler(
        ISensorMeasurementRepository sensorMeasurementRepository,
        IMapper mapper)
    {
        _sensorMeasurementRepository = sensorMeasurementRepository;
        _mapper = mapper;
    }

    public async Task<CreateSensorMeasurementResponse> Handle(
        CreateSensorMeasurementCommand request,
        CancellationToken cancellationToken)
    {
        var sensorMeasurementId = new SensorMeasurementId(Ulid.NewUlid());

        var sensorMeasurement = SensorMeasurement.Create(
            sensorMeasurementId,
            new DeviceId(request.DeviceId),
            request.SensorCode,
            request.MeasuredAt,
            request.Temperature,
            request.Vibration,
            request.Pressure,
            request.Humidity,
            request.IsFailure);

        await _sensorMeasurementRepository
            .AddAsync(sensorMeasurement, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateSensorMeasurementResponse>(sensorMeasurement);
    }
}