using CollectManagement.Application.Features.Types.Queries.GetPagedListType;
using CollectManagement.Application.Interfaces.Repositories.Types;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Types.Queries;

public class GetPagedListTypeQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paged_Types()
    {
        // Arrange
        var repository = new Mock<ITypeRepository>();
        var mapper = new Mock<IMapper>();


        repository
            .Setup(x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<CollectManagement.Domain.Types.Type>(),
                    0
                ));


        var handler = new GetPagedListTypeQueryHandler(
            repository.Object,
            mapper.Object);


        var query = new GetPagedListTypeQuery(
            null,
            null,
            null,
            1,
            10);



        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);



        // Assert
        result.Should().NotBeNull();


        repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}