namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetOneRoleUtilisateur;

public record GetOneRoleUtilisateurResponse(
    Ulid RoleUtilisateurId,
    string LibelleRoleUtilisateur,
    IList<GetOneRoleUtilisateurNavigation> Navigations);

public record GetOneRoleUtilisateurNavigation(
    string NavigationId,
    IList<int> Actions,
    IList<GetOneRoleUtilisateurSection> Sections
);

public record GetOneRoleUtilisateurSection(
    string SectionId,
    List<int> Actions
);