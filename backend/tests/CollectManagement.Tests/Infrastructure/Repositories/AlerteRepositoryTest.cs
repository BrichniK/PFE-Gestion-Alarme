using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class AlerteRepositoryTests
{
    private readonly Mock<IAlerteRepository> _repository;


    public AlerteRepositoryTests()
    {
        _repository = new Mock<IAlerteRepository>();
    }


    [Fact]
    public async Task GetOneAsync_Should_Return_Alerte()
    {
        // Arrange

        var id = new AlerteId(Ulid.NewUlid());

        var alerte = Alerte.Create(
            id,
            DateTime.UtcNow,
            new DeviceId(Ulid.NewUlid()),
            new TypeId(Ulid.NewUlid())
        );


        _repository
            .Setup(x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);



        // Act

        var result = await _repository.Object.GetOneAsync(
            id,
            CancellationToken.None);



        // Assert

        result.Should().NotBeNull();

        result.AlerteId
            .Should()
            .Be(id);


        _repository.Verify(
            x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }



    [Fact]
    public async Task ExistsByDeviceDateAndTypeAsync_Should_Return_True()
    {
        var deviceId = new DeviceId(Ulid.NewUlid());

        var typeId = new TypeId(Ulid.NewUlid());


        _repository
            .Setup(x => x.ExistsByDeviceDateAndTypeAsync(
                deviceId,
                It.IsAny<DateTime>(),
                typeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);



        var result =
            await _repository.Object.ExistsByDeviceDateAndTypeAsync(
                deviceId,
                DateTime.UtcNow,
                typeId,
                CancellationToken.None);



        result.Should().BeTrue();
    }
}