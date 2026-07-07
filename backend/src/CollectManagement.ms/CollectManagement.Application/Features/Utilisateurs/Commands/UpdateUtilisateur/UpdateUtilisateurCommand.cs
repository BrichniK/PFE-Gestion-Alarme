namespace CollectManagement.Application.Features.Utilisateurs.Commands.UpdateUtilisateur;

public record UpdateUtilisateurCommand(
    Ulid UtilisateurId,
    string NomUtilisateur,
    string Nom,
    string Prenom,
    string Email,
    string Password,
    Ulid? RoleUtilisateurId,
    bool IsActive,
    Ulid SocieteId
) : IRequest;