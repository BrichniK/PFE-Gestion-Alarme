namespace CollectManagement.Application.Contracts.Authentication;

public record AuthenticationResponse(
    Ulid UtilisateurId,
    string Nom,
    string NomUtilisateur,
    string Prenom,
    string Email,
    IList<AuthenticationNavigation> Navigations,
    string Token,
    Ulid SocieteId);
    
public record AuthenticationNavigation(
    string NavigationId,
    IList<int> Actions,
    IList<AuthenticationSection> Sections
);

public record AuthenticationSection(
    string SectionId,
    IList<int> Actions
);