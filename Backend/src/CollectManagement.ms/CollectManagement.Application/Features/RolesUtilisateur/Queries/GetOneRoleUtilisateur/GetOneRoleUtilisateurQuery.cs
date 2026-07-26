namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetOneRoleUtilisateur;

public record GetOneRoleUtilisateurQuery(
    Ulid RoleUtilisateurId):IRequest<GetOneRoleUtilisateurResponse>;