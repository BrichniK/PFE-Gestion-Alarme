using CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Shifts.Queries;

public class GetPagedListShiftQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paged_Shifts()
    {
        // Arrange
        var repository = new Mock<IShiftRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Shift>(), 0));

        mapper
            .Setup(x => x.Map<List<GetPagedListShiftDto>>(It.IsAny<List<Shift>>()))
            .Returns(new List<GetPagedListShiftDto>());

        var handler = new GetPagedListShiftQueryHandler(repository.Object, mapper.Object);
        var query = new GetPagedListShiftQuery(null, null, null, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        repository.Verify(
            x => x.GetPagedListAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
