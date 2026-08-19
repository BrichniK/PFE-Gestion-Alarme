using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Devices;

public class DeviceTests
{

    [Fact]
    public void Create_Should_Create_Device()
    {

        var id = new DeviceId(Ulid.NewUlid());


        var device = Device.Create(
            id,
            "Machine-01",
            "MAT-001",
            4
        );


        device.Should().NotBeNull();

        device.DeviceId.Should().Be(id);

        device.DeviceName.Should().Be("Machine-01");

        device.Matricule.Should().Be("MAT-001");

        device.NombreCapteur.Should().Be(4);

        device.IsOnline.Should().BeTrue();

        device.LastSeen.Should().NotBeNull();
    }


    [Fact]
    public void Update_Should_Modify_Device()
    {

        var device = Device.Create(
            new DeviceId(Ulid.NewUlid()),
            "OldName",
            "OLD-MAT",
            2
        );


        device.Update(
            "NewName",
            "NEW-MAT",
            8
        );


        device.DeviceName.Should().Be("NewName");

        device.Matricule.Should().Be("NEW-MAT");

        device.NombreCapteur.Should().Be(8);
    }


    [Fact]
    public void SetOnlineStatus_Should_Set_IsOnline_False()
    {

        var device = Device.Create(
            new DeviceId(Ulid.NewUlid()),
            "Machine-01",
            "MAT-001",
            4
        );


        device.SetOnlineStatus(false);


        device.IsOnline.Should().BeFalse();

        device.LastSeen.Should().NotBeNull();
    }


    [Fact]
    public void SetOnlineStatus_Should_Set_IsOnline_True()
    {

        var device = Device.Create(
            new DeviceId(Ulid.NewUlid()),
            "Machine-01",
            "MAT-001",
            4
        );

        device.SetOnlineStatus(false);


        device.SetOnlineStatus(true);


        device.IsOnline.Should().BeTrue();
    }
}
