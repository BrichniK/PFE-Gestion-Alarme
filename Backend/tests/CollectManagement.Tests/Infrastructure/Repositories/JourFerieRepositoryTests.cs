using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class JourFerieRepositoryTests
{
    private readonly Mock<IJourFerieRepository> _repository;

    public JourFerieRepositoryTests()
    {
        _repository = new Mock<IJourFerieRepository>();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_JourFerie()
    {
        // Arrange
        var id = new JourFerieId(Ulid.NewUlid());

        var jourFerie = JourFerie.Create(id, new DateTime(2026, 1, 1), "Jour de l'an");

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jourFerie);

        // Act
        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JourFerieId.Should().Be(id);
        result.Label.Should().Be("Jour de l'an");

        _repository.Verify(
            x => x.GetOneAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
