using CollectManagement.Application.Features.Alertes.Mapping;
using CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;
using CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Alertes.Mapping;

public class AlerteMappingTests
{
    private readonly TypeAdapterConfig _config;

    public AlerteMappingTests()
    {
        _config = new TypeAdapterConfig();
        _config.Scan(typeof(AlerteMapping).Assembly);
    }


    [Fact]
    public void ShouldMapAlerteToCreateAlerteResponse()
    {
        // Arrange
        var alerteId = new AlerteId(Ulid.NewUlid());

        var alerte = Alerte.Create(
            alerteId,
            DateTime.UtcNow,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );


        // Act
        var result = alerte.Adapt<CreateAlerteResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.AlerteId.Should()
            .Be(alerteId.Value);
    }



    [Fact]
    public void ShouldMapAlerteToGetOneAlerteResponse()
    {
        // Arrange
        var alerteId = new AlerteId(Ulid.NewUlid());

        var alerte = Alerte.Create(
            alerteId,
            DateTime.UtcNow,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );


        // Act
        var result = alerte.Adapt<GetOneAlerteResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.AlerteId.Should()
            .Be(alerteId.Value);

        result.Traiter.Should()
            .BeFalse();
    }
}