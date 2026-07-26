using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;

namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetListRoleUtilisateur;

public class GetListRoleUtilisateurQueryHandler  : IRequestHandler<GetListRoleUtilisateurQuery, GetListRoleutilisateurResponse>
{
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public GetListRoleUtilisateurQueryHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task<GetListRoleutilisateurResponse> Handle(
        GetListRoleUtilisateurQuery request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var (list, length) = await _roleUtilisateurRepository.GetPagedListAsync(
            request.Search,
            request.Sort,
            request.Order,
            request.Page,
            request.Size, cancellationToken);

        return new GetListRoleutilisateurResponse(
            list.ConvertAll(v=> new GetListRoleUtilisateurDto(
                v.RoleUtilisateurId.Value,
                v.LibelleRoleUtilisateur,
                v.Navigations.Select(s=> new GetListRoleUtilisateurNavigation(
                    s.NavigationId, 
                    s.Actions.Select(a=> (int)a).ToList())).ToList()
            )),
            length
        );
    }
}