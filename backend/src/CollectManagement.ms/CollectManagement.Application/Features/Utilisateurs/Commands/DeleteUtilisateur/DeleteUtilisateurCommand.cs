namespace CollectManagement.Application.Features.Utilisateurs.Commands.DeleteUtilisateur;

public record DeleteUtilisateurCommand(
    Ulid UtilisateurId
):IRequest;