namespace CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;

public record GetUtilisateurListResponse(
        IList<GetUtilisateurDto> Utilisateurs,
        int Length
    );
