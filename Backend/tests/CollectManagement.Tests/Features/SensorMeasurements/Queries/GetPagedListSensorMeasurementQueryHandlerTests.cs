using CollectManagement.Application.Features.SensorMeasurements.Mapping;
using CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Queries;

public class GetPagedListSensorMeasurementQueryHandlerTests
{

    private readonly Mock<ISensorMeasurementRepository> _repository;
    private readonly IMapper _mapper;


    public GetPagedListSensorMeasurementQueryHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();

        var config = new TypeAdapterConfig();
        new SensorMeasurementMapping().Register(config);
        _mapper = new Mapper(config);
    }


    [Fact]
    public async Task Handle_Should_Return_Empty_List()
    {

        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<Ulid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SensorMeasurement>().AsReadOnly() as IReadOnlyList<SensorMeasurement>, 0));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            null, null, null, null, 1, 10);


        var result = await handler.Handle(query, CancellationToken.None);


        result.Should().NotBeNull();

        result.Items.Should().BeEmpty();

        result.Length.Should().Be(0);
    }
}
