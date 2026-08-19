using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Alertes;

public class AlerteTests
{

    [Fact]
    public void Create_Should_Create_Alerte()
    {

        var alerteId    = new AlerteId(Ulid.NewUlid());
        var dispositifId = new DeviceId(Ulid.NewUlid());
        var typeId       = new TypeId(Ulid.NewUlid());
        var date         = DateTime.UtcNow;


        var alerte = Alerte.Create(
            alerteId,
            date,
            dispositifId,
            typeId
        );


        alerte.Should().NotBeNull();

        alerte.AlerteId.Should().Be(alerteId);

        alerte.Date.Should().Be(date);

        alerte.DispositifId.Should().Be(dispositifId);

        alerte.TypeId.Should().Be(typeId);

        alerte.Traiter.Should().BeFalse();
    }


    [Fact]
    public void Create_With_Traiter_Should_Set_Traiter_True()
    {

        var alerteId     = new AlerteId(Ulid.NewUlid());
        var dispositifId = new DeviceId(Ulid.NewUlid());
        var typeId       = new TypeId(Ulid.NewUlid());


        var alerte = Alerte.Create(
            alerteId,
            null,
            dispositifId,
            typeId,
            traiter: true
        );


        alerte.Traiter.Should().BeTrue();
    }


    [Fact]
    public void Update_Should_Modify_Alerte()
    {

        var alerte = Alerte.Create(
            new AlerteId(Ulid.NewUlid()),
            DateTime.UtcNow,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );

        var newDispositifId = new DeviceId(Ulid.NewUlid());
        var newTypeId       = new TypeId(Ulid.NewUlid());
        var newDate         = DateTime.UtcNow.AddDays(1);


        alerte.Update(
            newDate,
            newDispositifId,
            newTypeId
        );


        alerte.Date.Should().Be(newDate);

        alerte.DispositifId.Should().Be(newDispositifId);

        alerte.TypeId.Should().Be(newTypeId);
    }


    [Fact]
    public void SetTraiter_Should_Set_Traiter_To_True()
    {

        var alerte = Alerte.Create(
            new AlerteId(Ulid.NewUlid()),
            DateTime.UtcNow,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );


        alerte.SetTraiter();


        alerte.Traiter.Should().BeTrue();
    }


    [Fact]
    public void Create_With_Null_Date_Should_Be_Allowed()
    {

        var alerte = Alerte.Create(
            new AlerteId(Ulid.NewUlid()),
            null,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );


        alerte.Date.Should().BeNull();
    }
}
