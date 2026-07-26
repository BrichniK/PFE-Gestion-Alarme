using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.Enums;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.RolesUtilisateur.Commands.CreateRoleUtilisateur;

public class CreateRoleUtilisateurCommandHandler : IRequestHandler<CreateRoleUtilisateurCommand, CreateRoleUtilisateurResponse>
{
    
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public CreateRoleUtilisateurCommandHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task<CreateRoleUtilisateurResponse> Handle(
        CreateRoleUtilisateurCommand request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roleUtilisateurId = new RoleUtilisateurId(Ulid.NewUlid());

        var role = RoleUtilisateur.Create(roleUtilisateurId,
            request.LibelleRoleUtilisateur, 
            request.Navigations.ConvertAll(v=> Navigation.Create(
                v.NavigationId,
                roleUtilisateurId,
                v.Actions.ConvertAll(a => (NavigationAction)a),
                v.Sections.ConvertAll(s=> NavigationSection.Create(
                    s.SectionId, 
                    s.Actions.ConvertAll(a => (NavigationAction)a)))
            )));
        
        await _roleUtilisateurRepository.AddAsync(role, cancellationToken);
        
        return new CreateRoleUtilisateurResponse(roleUtilisateurId.Value);
    }
}