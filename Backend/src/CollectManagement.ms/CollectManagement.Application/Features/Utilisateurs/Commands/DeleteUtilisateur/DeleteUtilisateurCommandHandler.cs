using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.Utilisateurs.Commands.DeleteUtilisateur;

public class DeleteUtilisateurCommandHandler
    : IRequestHandler<DeleteUtilisateurCommand>
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public DeleteUtilisateurCommandHandler(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task Handle(DeleteUtilisateurCommand request, CancellationToken cancellationToken)
    {
        var utilisateurId = new UtilisateurId(request.UtilisateurId);

        await _utilisateurRepository
            .DeleteAsync(u => u.UtilisateurId.Equals(utilisateurId)
                , cancellationToken).ConfigureAwait(false);
    }
}