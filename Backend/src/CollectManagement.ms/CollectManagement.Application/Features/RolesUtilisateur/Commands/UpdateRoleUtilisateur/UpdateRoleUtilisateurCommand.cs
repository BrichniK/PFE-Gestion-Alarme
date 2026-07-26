namespace CollectManagement.Application.Features.RolesUtilisateur.Commands.UpdateRoleUtilisateur;

public record UpdateRoleUtilisateurCommand(
    Ulid RoleUtilisateurId,
    string LibelleRoleUtilisateur,
    List<UpdateRoleUtilisateurNavigation> Navigations
): IRequest;

public record UpdateRoleUtilisateurNavigation(
    string NavigationId,
    List<int> Actions,
    List<UpdateRoleUtilisateurSection> Sections
);
    
public record UpdateRoleUtilisateurSection(
    string SectionId,
    List<int> Actions
);