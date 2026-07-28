using CollectManagement.Application.Features.Alertes.Mapping;
using CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Alertes;

public class GetOneAlerteQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnAlerteWhenExists()
    {
        // Arrange
        var repository = new Mock<IAlerteRepository>();

        // Configuration Mapster
        var config = new TypeAdapterConfig();
        config.Scan(typeof(AlerteMapping).Assembly);

        var mapper = new Mapper(config);


        var alerteId = new AlerteId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var typeId = new TypeId(Ulid.NewUlid());


        var alerte = Alerte.Create(
            alerteId,
            DateTime.UtcNow,
            deviceId,
            typeId);


        repository
            .Setup(x => x.GetOneAsync(
                alerteId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);


        var handler = new GetOneAlerteQueryHandler(
            repository.Object,
            mapper);


        var query = new GetOneAlerteQuery(
            alerteId.Value);


        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);


        // Assert
        result.Should().NotBeNull();

        result.AlerteId.Should().Be(alerteId.Value);

        repository.Verify(
            x => x.GetOneAsync(
                alerteId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}