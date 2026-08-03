using CollectManagement.Application.Features.Devices.Mapping;
using CollectManagement.Application.Features.Devices.Commands.CreateDevice;
using CollectManagement.Application.Features.Devices.Queries.GetOneDevice;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Devices.Mapping;

public class DeviceMappingTests
{
    private readonly TypeAdapterConfig _config;

    public DeviceMappingTests()
    {
        _config = new TypeAdapterConfig();
        _config.Scan(typeof(DeviceMapping).Assembly);
    }


    [Fact]
    public void Should_Map_Device_To_CreateDeviceResponse()
    {
        var deviceId = new DeviceId(Ulid.NewUlid());

        var device = Device.Create(
            deviceId,
            "Device-Test",
            "MAT-001",
            4
        );

        var result = device.Adapt<CreateDeviceResponse>(_config);

        result.Should().NotBeNull();
        result.DeviceId.Should().Be(deviceId.Value);
    }



    [Fact]
    public void Should_Map_Device_To_GetOneDeviceResponse()
    {
        var deviceId = new DeviceId(Ulid.NewUlid());

        var device = Device.Create(
            deviceId,
            "Device-Test",
            "MAT-001",
            4
        );

        var result = device.Adapt<GetOneDeviceResponse>(_config);

        result.Should().NotBeNull();

        result.DeviceId.Should().Be(deviceId.Value);
        result.DeviceName.Should().Be("Device-Test");
        result.Matricule.Should().Be("MAT-001");
    }
}