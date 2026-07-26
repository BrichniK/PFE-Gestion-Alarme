namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetListRoleUtilisateur;

public record GetListRoleUtilisateurQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
):IRequest<GetListRoleutilisateurResponse>;