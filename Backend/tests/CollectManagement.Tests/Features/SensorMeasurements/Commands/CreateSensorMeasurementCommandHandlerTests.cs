using CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;
using CollectManagement.Application.Features.SensorMeasurements.Mapping;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Commands;

public class CreateSensorMeasurementCommandHandlerTests
{

    private readonly Mock<ISensorMeasurementRepository> _repository;
    private readonly IMapper _mapper;


    public CreateSensorMeasurementCommandHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();

        var config = new TypeAdapterConfig();
        new SensorMeasurementMapping().Register(config);
        _mapper = new Mapper(config);
    }


    [Fact]
    public async Task Handle_Should_Create_SensorMeasurement()
    {

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        var date    = DateTime.UtcNow;
        var command = new CreateSensorMeasurementCommand(
            Ulid.NewUlid(),
            "CAPTEUR-01",
            date,
            25.5,
            0.3,
            1013.0,
            60.0,
            false
        );


        var result = await handler.Handle(command, CancellationToken.None);


        result.Should().NotBeNull();

        result.SensorMeasurementId.Should().NotBe(Ulid.Empty);

        result.SensorCode.Should().Be("CAPTEUR-01");

        result.Temperature.Should().Be(25.5);

        result.IsFailure.Should().BeFalse();

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Handle_Should_Create_SensorMeasurement_With_IsFailure_True()
    {

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        var command = new CreateSensorMeasurementCommand(
            Ulid.NewUlid(),
            "CAPTEUR-02",
            DateTime.UtcNow,
            null, null, null, null,
            IsFailure: true
        );


        var result = await handler.Handle(command, CancellationToken.None);


        result.IsFailure.Should().BeTrue();

        result.Temperature.Should().BeNull();
    }
}
