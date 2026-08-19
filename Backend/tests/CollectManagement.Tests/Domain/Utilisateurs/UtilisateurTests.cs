using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Utilisateurs;

public class UtilisateurTests
{
    private static Utilisateur BuildUtilisateur(
        UtilisateurId? id       = null,
        RoleUtilisateurId? role = null)
    {
        return Utilisateur.Create(
            id ?? new UtilisateurId(Ulid.NewUlid()),
            "jdoe",
            "Doe",
            "John",
            "jdoe@example.com",
            "hashed_password",
            role,
            true,
            new SocieteId(Ulid.NewUlid()));
    }

    [Fact]
    public void Create_Should_Create_Utilisateur()
    {
        var id = new UtilisateurId(Ulid.NewUlid());

        var u = BuildUtilisateur(id);

        u.Should().NotBeNull();
        u.UtilisateurId.Should().Be(id);
        u.NomUtilisateur.Should().Be("jdoe");
        u.Nom.Should().Be("Doe");
        u.Prenom.Should().Be("John");
        u.Email.Should().Be("jdoe@example.com");
        u.Password.Should().Be("hashed_password");
        u.IsActive.Should().BeTrue();
        u.RoleUtilisateurId.Should().BeNull();
    }

    [Fact]
    public void Create_With_Role_Should_Set_RoleId()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());

        var u = BuildUtilisateur(role: roleId);

        u.RoleUtilisateurId.Should().Be(roleId);
    }

    [Fact]
    public void Update_Should_Modify_Utilisateur()
    {
        var u        = BuildUtilisateur();
        var newRole  = new RoleUtilisateurId(Ulid.NewUlid());
        var newSoc   = new SocieteId(Ulid.NewUlid());

        u.Update(
            "jsmith",
            "Smith",
            "Jane",
            "jsmith@example.com",
            "new_hash",
            newRole,
            false,
            newSoc);

        u.NomUtilisateur.Should().Be("jsmith");
        u.Nom.Should().Be("Smith");
        u.Prenom.Should().Be("Jane");
        u.IsActive.Should().BeFalse();
        u.RoleUtilisateurId.Should().Be(newRole);
        u.SocieteId.Should().Be(newSoc);
    }
}
