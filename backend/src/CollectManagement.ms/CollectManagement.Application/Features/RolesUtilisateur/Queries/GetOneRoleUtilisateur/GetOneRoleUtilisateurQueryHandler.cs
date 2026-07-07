using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.RolesUtilisateur.Queries.GetOneRoleUtilisateur;

public class GetOneRoleUtilisateurQueryHandler  : IRequestHandler<GetOneRoleUtilisateurQuery, GetOneRoleUtilisateurResponse>
{
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public GetOneRoleUtilisateurQueryHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task<GetOneRoleUtilisateurResponse> Handle(
        GetOneRoleUtilisateurQuery request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var roleUtilisateurId = new RoleUtilisateurId(request.RoleUtilisateurId);
        
        var role = await _roleUtilisateurRepository.GetOneAsync(roleUtilisateurId, cancellationToken)
                   ?? throw new NotFoundException("Role introuvable");

        return new GetOneRoleUtilisateurResponse(
            role.RoleUtilisateurId.Value,
            role.LibelleRoleUtilisateur,
            role.Navigations.Select(s=> new GetOneRoleUtilisateurNavigation(
                s.NavigationId,
                s.Actions.Select(a=> (int)a).ToList(),
                s.Sections.Select(section=> new GetOneRoleUtilisateurSection(
                    section.SectionId,
                    section.Actions.Select(a=> (int)a).ToList()
                )).ToList()
            )).ToList()
        );
    }
}