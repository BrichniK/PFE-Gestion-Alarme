using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;

namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetAllRoleUtilisateur;

public class GetAllRoleUtilisateurQueryHandler : IRequestHandler<GetAllRoleUtilisateurQuery, List<GetAllRoleUtilisateurResponse>>
{
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public GetAllRoleUtilisateurQueryHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task<List<GetAllRoleUtilisateurResponse>> Handle(
        GetAllRoleUtilisateurQuery request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var list = await _roleUtilisateurRepository.GetAllAsync(cancellationToken);

        return list.Select(v=> new GetAllRoleUtilisateurResponse(
            v.RoleUtilisateurId.Value,
            v.LibelleRoleUtilisateur,
            v.Navigations.Select(s=> new GetAllRoleUtilisateurNavigation(
                s.NavigationId, 
                s.Actions.Select(a=> (int)a).ToList())).ToList()
        )).ToList();
    }
}