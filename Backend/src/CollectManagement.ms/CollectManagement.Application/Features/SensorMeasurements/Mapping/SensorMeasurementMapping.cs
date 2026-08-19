using CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;
using CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;
using CollectManagement.Domain.SensorMeasurements;

namespace CollectManagement.Application.Features.SensorMeasurements.Mapping;

public class SensorMeasurementMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SensorMeasurement, CreateSensorMeasurementResponse>()
            .Map(d => d.SensorMeasurementId, s => s.SensorMeasurementId.Value)
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.SensorCode, s => s.SensorCode)
            .Map(d => d.MeasuredAt, s => s.MeasuredAt)
            .Map(d => d.Temperature, s => s.Temperature)
            .Map(d => d.Vibration, s => s.Vibration)
            .Map(d => d.Pressure, s => s.Pressure)
            .Map(d => d.Humidity, s => s.Humidity)
            .Map(d => d.IsFailure, s => s.IsFailure);

        config.NewConfig<SensorMeasurement, GetPagedListSensorMeasurementDto>()
            .Map(d => d.SensorMeasurementId, s => s.SensorMeasurementId.Value)
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.SensorCode, s => s.SensorCode)
            .Map(d => d.MeasuredAt, s => s.MeasuredAt)
            .Map(d => d.Temperature, s => s.Temperature)
            .Map(d => d.Vibration, s => s.Vibration)
            .Map(d => d.Pressure, s => s.Pressure)
            .Map(d => d.Humidity, s => s.Humidity)
            .Map(d => d.IsFailure, s => s.IsFailure);
    }
}