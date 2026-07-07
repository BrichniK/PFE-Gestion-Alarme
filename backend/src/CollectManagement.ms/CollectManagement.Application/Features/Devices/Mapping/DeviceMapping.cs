using CollectManagement.Application.Features.Devices.Commands.CreateDevice;
using CollectManagement.Application.Features.Devices.Queries.GetOneDevice;
using CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;
using CollectManagement.Domain.Devices;

namespace CollectManagement.Application.Features.Devices.Mapping;

public class DeviceMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Device, CreateDeviceResponse>()
            .Map(d => d.DeviceId, s => s.DeviceId.Value);

        config.NewConfig<Device, GetPagedListDeviceDto>()
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.DeviceName, s => s.DeviceName)
            .Map(d => d.Matricule, s => s.Matricule)
            .Map(d => d.NombreCapteur, s => s.NombreCapteur)
            .Map(d => d.IsOnline, s => s.IsOnline)
            .Map(d => d.LastSeen, s => s.LastSeen);

        config.NewConfig<Device, GetOneDeviceResponse>()
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.DeviceName, s => s.DeviceName)
            .Map(d => d.Matricule, s => s.Matricule)
            .Map(d => d.NombreCapteur, s => s.NombreCapteur)
            .Map(d => d.IsOnline, s => s.IsOnline)
            .Map(d => d.LastSeen, s => s.LastSeen);
    }
}
