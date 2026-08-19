using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Groupes;

public class GroupeTests
{

    [Fact]
    public void Create_Should_Create_Groupe()
    {

        var id          = new GroupeId(Ulid.NewUlid());
        var employeeIds = new List<Ulid> { Ulid.NewUlid(), Ulid.NewUlid() };


        var groupe = Groupe.Create(
            id,
            "Equipe A",
            "#FF5733",
            employeeIds
        );


        groupe.Should().NotBeNull();

        groupe.GroupeId.Should().Be(id);

        groupe.Nom.Should().Be("Equipe A");

        groupe.Color.Should().Be("#FF5733");

        groupe.EmployeeIds.Should().HaveCount(2);
    }


    [Fact]
    public void Create_With_Empty_EmployeeIds_Should_Be_Allowed()
    {

        var groupe = Groupe.Create(
            new GroupeId(Ulid.NewUlid()),
            "Equipe B",
            "#000000",
            new List<Ulid>()
        );


        groupe.EmployeeIds.Should().BeEmpty();
    }
}
