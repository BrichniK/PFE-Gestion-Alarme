using CollectManagement.Application.Features.Types.Commands.CreateType;
using CollectManagement.Application.Features.Types.Mapping;
using CollectManagement.Application.Features.Types.Queries.GetOneType;
using FluentAssertions;
using Mapster;
using Type = CollectManagement.Domain.Types.Type;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Tests.Features.Types.Mapping;

public class TypeMappingTests
{
    private readonly TypeAdapterConfig _config;


    public TypeMappingTests()
    {
        _config = new TypeAdapterConfig();

        _config.Scan(typeof(TypeMapping).Assembly);
    }


    [Fact]
    public void Should_Map_Type_To_CreateTypeResponse()
    {
        // Arrange
        var typeId = new TypeId(Ulid.NewUlid());

        var type = Type.Create(
            typeId,
            "ELEC",
            "Electricite",
            30
        );


        // Act
        var result = type.Adapt<CreateTypeResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.TypeId
            .Should()
            .Be(typeId.Value);
    }



    [Fact]
    public void Should_Map_Type_To_GetOneTypeResponse()
    {
        // Arrange
        var typeId = new TypeId(Ulid.NewUlid());

        var type = Type.Create(
            typeId,
            "ELEC",
            "Electricite",
            30
        );


        // Act
        var result = type.Adapt<GetOneTypeResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.TypeId.Should().Be(typeId.Value);
        result.Code.Should().Be("ELEC");
        result.Label.Should().Be("Electricite");
        result.DureeNominal.Should().Be(30);
    }
}