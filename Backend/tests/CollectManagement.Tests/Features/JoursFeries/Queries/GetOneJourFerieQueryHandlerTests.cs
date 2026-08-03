using CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.JoursFeries.Queries;

public class GetOneJourFerieQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_JourFerie()
    {
        // Arrange
        var repository = new Mock<IJourFerieRepository>();
        var mapper = new Mock<IMapper>();

        var jourFerieId = new JourFerieId(Ulid.NewUlid());

        var jourFerie = JourFerie.Create(
            jourFerieId,
            new DateTime(2026, 1, 1),
            "Jour de l'an"
        );

        var response = new GetOneJourFerieResponse(
            jourFerieId.Value,
            new DateTime(2026, 1, 1),
            "Jour de l'an"
        );

        repository
            .Setup(x => x.GetOneAsync(jourFerieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jourFerie);

        mapper
            .Setup(x => x.Map<GetOneJourFerieResponse>(It.IsAny<JourFerie>()))
            .Returns(response);

        var handler = new GetOneJourFerieQueryHandler(repository.Object, mapper.Object);
        var query = new GetOneJourFerieQuery(jourFerieId.Value);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JourFerieId.Should().Be(jourFerieId.Value);
        result.Label.Should().Be("Jour de l'an");

        repository.Verify(
            x => x.GetOneAsync(jourFerieId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
