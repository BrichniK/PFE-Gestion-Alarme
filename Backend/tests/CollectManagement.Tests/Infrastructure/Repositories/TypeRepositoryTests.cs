using CollectManagement.Application.Interfaces.Repositories.Types;
using TypeEntity = CollectManagement.Domain.Types.Type;

using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Moq;


namespace CollectManagement.Tests.Infrastructure.Repositories;


public class TypeRepositoryTests
{

    private readonly Mock<ITypeRepository> _repository;


    public TypeRepositoryTests()
    {
        _repository = new Mock<ITypeRepository>();
    }



    [Fact]
    public async Task GetOneAsync_Should_Return_Type()
    {

        var id =
            new TypeId(Ulid.NewUlid());


        var type =
            TypeEntity.Create(
                id,
                "Temperature",
                "ff",
                12
            );



        _repository
            .Setup(x=>x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);



        var result =
            await _repository.Object.GetOneAsync(
                id,
                CancellationToken.None);



        result.Should()
            .NotBeNull();


        result.TypeId
            .Should()
            .Be(id);

    }

}