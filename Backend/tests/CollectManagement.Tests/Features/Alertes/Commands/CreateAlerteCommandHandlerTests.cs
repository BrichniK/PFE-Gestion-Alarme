using CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;
using CollectManagement.Application.Features.Alertes.Mapping;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Alertes;

public class CreateAlerteCommandHandlerTests
{
    [Fact]
    public async Task HandleShouldCreateAlerteWhenCommandIsValid()
    {
       
        var repository = new Mock<IAlerteRepository>();

        var config = new TypeAdapterConfig();
        new AlerteMapping().Register(config);

        var mapper = new Mapper(config);

        var handler = new CreateAlerteCommandHandler(
            repository.Object,
            mapper);

        var command = new CreateAlerteCommand(
            DateTime.UtcNow,
            Ulid.NewUlid(),
            Ulid.NewUlid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.Verify(
            x => x.AddAsync(
                It.IsAny<CollectManagement.Domain.Alertes.Alerte>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNull();
        result.AlerteId.Should().NotBe(Ulid.Empty);
    }
}