using CollectManagement.Domain.Types;
using CollectManagement.Domain.Types.ValueObjects;
using FluentAssertions;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Tests.Features.Domain.Types;


public class TypeTests
{

    [Fact]
    public void Create_Should_Create_Type()
    {

        var id = new TypeId(Ulid.NewUlid());


        var type = Type.Create(
            id,
            "TEMP",
            "Temperature",
            60
        );


        type.Should().NotBeNull();

        type.TypeId.Should().Be(id);

        type.Code.Should().Be("TEMP");

        type.Label.Should().Be("Temperature");

        type.DureeNominal.Should().Be(60);
    }




    [Fact]
    public void Update_Should_Modify_Type()
    {

        var type = Type.Create(
            new TypeId(Ulid.NewUlid()),
            "OLD",
            "Old Label",
            10
        );


        type.Update(
            "NEW",
            "New Label",
            20
        );


        type.Code.Should().Be("NEW");

        type.Label.Should().Be("New Label");

        type.DureeNominal.Should().Be(20);
    }

}