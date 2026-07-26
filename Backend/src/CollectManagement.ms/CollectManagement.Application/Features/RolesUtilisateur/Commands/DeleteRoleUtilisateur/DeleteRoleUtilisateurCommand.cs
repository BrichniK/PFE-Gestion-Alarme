namespace CollectManagement.Application.Features.RolesUtilisateur.Commands.DeleteRoleUtilisateur;

public record DeleteRoleUtilisateurCommand(
    Ulid RoleUtilisateurId) : IRequest;