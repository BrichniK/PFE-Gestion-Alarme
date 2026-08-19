using CollectManagement.Application.Common;
using CollectManagement.Domain.Utilisateurs.Enums;
using FluentAssertions;

namespace CollectManagement.Tests.Common;

public class EnumHelperTests
{
    [Fact]
    public void SmartEnumToList_Should_Return_All_Enum_Values()
    {
        var result = EnumHelper.SmartEnumToList<UtilisateurRole>();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(Enum.GetValues<UtilisateurRole>().Length);
    }

    [Fact]
    public void SmartEnumToList_Should_Return_EnumInfo_With_Id()
    {
        var result = EnumHelper.SmartEnumToList<UtilisateurRole>();

        result.Should().AllSatisfy(x => x.Id.Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public void SmartEnumToList_Should_Return_EnumInfo_With_Value()
    {
        var result = EnumHelper.SmartEnumToList<UtilisateurRole>();

        result.Should().AllSatisfy(x => x.Value.Should().NotBeNull());
    }

    [Fact]
    public void SmartEnumToList_NavigationAction_Should_Return_Values()
    {
        var result = EnumHelper.SmartEnumToList<NavigationAction>();

        result.Should().NotBeEmpty();
    }
}
