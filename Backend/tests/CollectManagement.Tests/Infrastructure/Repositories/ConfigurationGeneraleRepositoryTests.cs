using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class ConfigurationGeneraleRepositoryTests
{
    private readonly Mock<IConfigurationGeneraleRepository> _repository;

    public ConfigurationGeneraleRepositoryTests()
    {
        _repository = new Mock<IConfigurationGeneraleRepository>();
    }

    [Fact]
    public async Task GetConfigurationAsync_Should_Return_Config()
    {
        // Arrange
        var id = new ConfigurationGeneraleId(Ulid.NewUlid());
        var config = ConfigurationGenerale.Create(id, true, false, true, false, 1.0, 1.5, 2.0, 2.5);

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _repository.Object.GetConfigurationAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ConfigurationGeneraleId.Should().Be(id);
        result.EcraserEmployeMaintenance.Should().BeTrue();

        _repository.Verify(
            x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetConfigurationAsync_Should_Return_Null_When_No_Config()
    {
        // Arrange
        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationGenerale?)null);

        // Act
        var result = await _repository.Object.GetConfigurationAsync(CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
