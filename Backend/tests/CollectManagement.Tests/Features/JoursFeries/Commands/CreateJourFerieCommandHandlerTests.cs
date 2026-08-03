using CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.JoursFeries.Commands;

public class CreateJourFerieCommandHandlerTests
{
    private readonly Mock<IJourFerieRepository> _repository;
    private readonly Mock<IMapper> _mapper;
    private readonly CreateJourFerieCommandHandler _handler;

    public CreateJourFerieCommandHandlerTests()
    {
        _repository = new Mock<IJourFerieRepository>();
        _mapper = new Mock<IMapper>();
        _handler = new CreateJourFerieCommandHandler(_repository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_JourFerie()
    {
        // Arrange
        var response = new CreateJourFerieResponse(Ulid.NewUlid());

        _mapper
            .Setup(x => x.Map<CreateJourFerieResponse>(It.IsAny<JourFerie>()))
            .Returns(response);

        var command = new CreateJourFerieCommand(
            new DateTime(2026, 1, 1),
            "Jour de l'an"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JourFerieId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<JourFerie>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
