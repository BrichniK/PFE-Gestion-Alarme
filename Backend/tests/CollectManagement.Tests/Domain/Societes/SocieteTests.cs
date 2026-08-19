using CollectManagement.Domain.Common;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Societes;

public class SocieteTests
{

    [Fact]
    public void Create_Should_Create_Societe()
    {

        var id   = new SocieteId(Ulid.NewUlid());
        var date = new DateTime(2020, 1, 1);


        var societe = Societe.Create(
            id,
            "/logos/societe.png",
            "CST Company",
            "123456A/P/000",
            "RNE-001",
            50000m,
            date,
            "71000000",
            "72000000",
            "71000001",
            null,
            "contact@cst.com",
            "Tunis, Tunisie",
            "CST-001"
        );


        societe.Should().NotBeNull();

        societe.SocieteId.Should().Be(id);

        societe.Nom.Should().Be("CST Company");

        societe.MatriculeFiscal.Should().Be("123456A/P/000");

        societe.Capital.Should().Be(50000m);

        societe.DateOverture.Should().Be(date);

        societe.Email.Should().Be("contact@cst.com");

        societe.Fax2.Should().BeNull();
    }


    [Fact]
    public void Update_Should_Modify_Societe()
    {

        var societe = Societe.Create(
            new SocieteId(Ulid.NewUlid()),
            null, "Old Name", null, null, null,
            DateTime.UtcNow, null, null, null, null, null, null, null
        );


        societe.Update(
            "/new-logo.png",
            "New Name",
            "MAT-NEW",
            null,
            100000m,
            new DateTime(2021, 6, 1),
            "70000000",
            null, null, null,
            "new@email.com",
            "Sfax",
            "NEW-001"
        );


        societe.Nom.Should().Be("New Name");

        societe.LogoPath.Should().Be("/new-logo.png");

        societe.Capital.Should().Be(100000m);

        societe.Email.Should().Be("new@email.com");
    }
}
