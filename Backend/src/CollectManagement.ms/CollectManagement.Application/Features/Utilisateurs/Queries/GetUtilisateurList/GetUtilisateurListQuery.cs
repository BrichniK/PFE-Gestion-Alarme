namespace CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;

public record GetUtilisateurListQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
):IRequest<GetUtilisateurListResponse>;