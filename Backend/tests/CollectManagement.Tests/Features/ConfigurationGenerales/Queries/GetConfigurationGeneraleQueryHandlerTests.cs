using CollectManagement.Application.Features.ConfigurationGenerales.Queries.GetConfigurationGenerale;
using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.ConfigurationGenerales.Queries;

public class GetConfigurationGeneraleQueryHandlerTests
{
    private readonly Mock<IConfigurationGeneraleRepository> _repository;
    private readonly GetConfigurationGeneraleQueryHandler _handler;

    public GetConfigurationGeneraleQueryHandlerTests()
    {
        _repository = new Mock<IConfigurationGeneraleRepository>();
        _handler = new GetConfigurationGeneraleQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Default_When_No_Config_Exists()
    {
        // Arrange
        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationGenerale?)null);

        // Act
        var result = await _handler.Handle(new GetConfigurationGeneraleQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationGeneraleId.Should().BeNull();
        result.DiagnostiqueObligatoire.Should().BeTrue();
        result.MonitoringPourcentageSurSommeDurees.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Config_When_Exists()
    {
        // Arrange
        var configId = new ConfigurationGeneraleId(Ulid.NewUlid());
        var config = ConfigurationGenerale.Create(configId, true, false, true, false, 1.0, 1.5, 2.0, 2.5);

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _handler.Handle(new GetConfigurationGeneraleQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationGeneraleId.Should().Be(configId.Value);
        result.EcraserEmployeMaintenance.Should().BeTrue();
        result.AccepterSeulementEmployesPlanifies.Should().BeFalse();
        result.CoefficientGaugeD1.Should().Be(1.0);
        result.CoefficientGaugeD2.Should().Be(1.5);

        _repository.Verify(
            x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
