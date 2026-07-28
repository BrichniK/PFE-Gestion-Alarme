using CollectManagement.Application.Features.Types.Mapping;
using CollectManagement.Application.Features.Types.Queries.GetOneType;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Types.Queries;

public class GetOneTypeQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Type_When_Exists()
    {
        // Arrange

        var repository = new Mock<ITypeRepository>();


        var config = new TypeAdapterConfig();

        config.Scan(typeof(TypeMapping).Assembly);


        var mapper = new Mapper(config);



        var typeId = new TypeId(
            Ulid.NewUlid()
        );


        var type =
            CollectManagement.Domain.Types.Type.Create(
                typeId,
                "ELEC",
                "Electricite",
                30
            );



        repository
            .Setup(x => x.GetOneAsync(
                typeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);



        var handler = new GetOneTypeQueryHandler(
            repository.Object,
            mapper);



        var query = new GetOneTypeQuery(
            typeId.Value
        );



        // Act

        var result = await handler.Handle(
            query,
            CancellationToken.None);



        // Assert

        result.Should().NotBeNull();

        result.TypeId.Should().Be(typeId.Value);
        result.Code.Should().Be("ELEC");


        repository.Verify(
            x => x.GetOneAsync(
                typeId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}