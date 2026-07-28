using CollectManagement.Application.Features.Alertes.Commands.UpdateAlerte;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Alertes;

public class UpdateAlerteCommandHandlerTests
{
    private readonly Mock<IAlerteRepository> _repository;
    private readonly UpdateAlerteCommandHandler _handler;


    public UpdateAlerteCommandHandlerTests()
    {
        _repository = new Mock<IAlerteRepository>();

        _handler = new UpdateAlerteCommandHandler(
            _repository.Object
        );
    }


    [Fact]
    public async Task Handle_Should_Update_Alerte_When_Exists()
    {
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



        var command = new UpdateAlerteCommand(
            id.Value,
            DateTime.UtcNow,
            Ulid.NewUlid(),
            Ulid.NewUlid()
        );


        await _handler.Handle(
            command,
            CancellationToken.None);


        _repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<Alerte>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}