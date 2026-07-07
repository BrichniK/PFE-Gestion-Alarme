namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetAllRoleUtilisateur;

public record GetAllRoleUtilisateurResponse(
    Ulid RoleUtilisateurId,
    string LibelleRoleUtilisateur,
    List<GetAllRoleUtilisateurNavigation> Navigations);

public record GetAllRoleUtilisateurNavigation(
    string NavigationId,
    List<int> Actions
);