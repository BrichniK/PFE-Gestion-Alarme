using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.RolesUtilisateur.Commands.DeleteRoleUtilisateur;

public class DeleteRoleUtilisateurCommandHandler : IRequestHandler<DeleteRoleUtilisateurCommand>
{
    private readonly IRoleUtilisateurRepository _roleUtilisateurRepository;

    public DeleteRoleUtilisateurCommandHandler(IRoleUtilisateurRepository roleUtilisateurRepository)
    {
        _roleUtilisateurRepository = roleUtilisateurRepository;
    }

    public async Task Handle(
        DeleteRoleUtilisateurCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var roleUtilisateurId = new RoleUtilisateurId(request.RoleUtilisateurId);

        _ = await _roleUtilisateurRepository.GetOneAsync(
                roleUtilisateurId, cancellationToken)
            ?? throw new NotFoundException("Role introuvable");

        try
        {
            await _roleUtilisateurRepository.DeleteAsync(w=>w.RoleUtilisateurId.Equals(roleUtilisateurId)
                , cancellationToken);
        }
        catch 
        {
            throw new ForbiddenException("Impossible de supprimer le role");
        }

    }
}