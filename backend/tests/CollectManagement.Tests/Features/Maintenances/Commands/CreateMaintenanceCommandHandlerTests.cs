using CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Maintenances;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Maintenances.Commands;

public class CreateMaintenanceCommandHandlerTests
{
    private readonly Mock<IMaintenanceRepository> _repository;
    private readonly Mock<IMapper> _mapper;


    public CreateMaintenanceCommandHandlerTests()
    {
        _repository = new Mock<IMaintenanceRepository>();
        _mapper = new Mock<IMapper>();
    }


    [Fact]
    public async Task Handle_Should_Create_Maintenance()
    {
        var response = new CreateMaintenanceResponse(
            Ulid.NewUlid()
        );


        _mapper
            .Setup(x => x.Map<CreateMaintenanceResponse>(
                It.IsAny<Maintenance>()))
            .Returns(response);



        var handler = new CreateMaintenanceCommandHandler(
            _repository.Object,
            _mapper.Object);



        var command = new CreateMaintenanceCommand(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            null,
            "Maintenance test"
        );


        var result = await handler.Handle(
            command,
            CancellationToken.None);



        result.Should().NotBeNull();


        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}