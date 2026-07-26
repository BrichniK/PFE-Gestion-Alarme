namespace CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;

public record GetUtilisateurDto(
    Ulid UtilisateurId,
    string NomUtilisateur,
    string Nom,
    string Prenom,
    string Email,
    Ulid? RoleUtilisateurId,
    string? LibelleRoleUtilisateur,
    bool IsActive,
    Ulid SocieteId
    );