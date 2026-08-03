using CollectManagement.Application.Features.ConfigurationGenerales.Commands.UpdateConfigurationGenerale;
using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.ConfigurationGenerales.Commands;

public class UpdateConfigurationGeneraleCommandHandlerTests
{
    private readonly Mock<IConfigurationGeneraleRepository> _repository;
    private readonly UpdateConfigurationGeneraleCommandHandler _handler;

    public UpdateConfigurationGeneraleCommandHandlerTests()
    {
        _repository = new Mock<IConfigurationGeneraleRepository>();
        _handler = new UpdateConfigurationGeneraleCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Config_When_None_Exists()
    {
        // Arrange
        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationGenerale?)null);

        var command = new UpdateConfigurationGeneraleCommand(
            true, false, true, false, 1.0, 1.5, 2.0, 2.5
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationGeneraleId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<ConfigurationGenerale>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_Config_When_Exists()
    {
        // Arrange
        var configId = new ConfigurationGeneraleId(Ulid.NewUlid());
        var existing = ConfigurationGenerale.Create(configId, false, false, false, false, 1, 1, 1, 1);

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repository
            .Setup(x => x.UpdateBulkAsync(It.IsAny<ConfigurationGenerale>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateConfigurationGeneraleCommand(
            true, true, true, true, 1.0, 1.5, 2.0, 2.5
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationGeneraleId.Should().Be(configId.Value);

        _repository.Verify(
            x => x.UpdateBulkAsync(It.IsAny<ConfigurationGenerale>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
