using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.Enums;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.RolesUtilisateur.Commands.UpdateRoleUtilisateur;

public class UpdateRoleUtilisateurCommandHandler : IRequestHandler<UpdateRoleUtilisateurCommand>
{
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public UpdateRoleUtilisateurCommandHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task Handle(
        UpdateRoleUtilisateurCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roleUtilisateurId = new RoleUtilisateurId(request.RoleUtilisateurId);

        var role = await _roleUtilisateurRepository.GetOneAsync(
                       roleUtilisateurId, cancellationToken)
                   ?? throw new NotFoundException("Role introuvable");

        _roleUtilisateurRepository.Attach(role);
        
        role.Update(
            request.LibelleRoleUtilisateur, 
            request.Navigations.ConvertAll(v=> Navigation.Create(
                v.NavigationId,
                roleUtilisateurId,
                v.Actions.ConvertAll(a => (NavigationAction)a),
                v.Sections.ConvertAll(s=> NavigationSection.Create(
                    s.SectionId, 
                    s.Actions.ConvertAll(a => (NavigationAction)a)))
            )));
    }
}