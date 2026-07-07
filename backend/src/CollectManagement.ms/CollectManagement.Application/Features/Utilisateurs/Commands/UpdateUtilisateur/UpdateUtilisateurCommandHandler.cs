using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Utilisateurs.Commands.UpdateUtilisateur;

public class UpdateUtilisateurCommandHandler
    : IRequestHandler<UpdateUtilisateurCommand>
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IPasswordService _passwordService;
    private readonly ISocieteRepository _societeRepository;

    public UpdateUtilisateurCommandHandler(IUtilisateurRepository utilisateurRepository, IPasswordService passwordService, ISocieteRepository societeRepository)
    {
        _utilisateurRepository = utilisateurRepository;
        _passwordService = passwordService;
        _societeRepository = societeRepository;
    }

    public async Task Handle(UpdateUtilisateurCommand request, CancellationToken cancellationToken)
    {
        var utilisateurId = new UtilisateurId(request.UtilisateurId);

        var utilisateur = await _utilisateurRepository.GetAsync(
            w => w.UtilisateurId.Equals(utilisateurId),
            cancellationToken
        ).ConfigureAwait(false) ?? throw new NotFoundException("Utilisateur NotFound");

        _utilisateurRepository.Attach(utilisateur);

        UpdateUtilisateur(request, utilisateur, utilisateurId, _passwordService
        );

    }

    private static void UpdateUtilisateur(
        UpdateUtilisateurCommand request,
        Utilisateur utilisateur,
        UtilisateurId utilisateurId,
        IPasswordService passwordService
    )
    {
        utilisateur.Update(
            request.NomUtilisateur,
            request.Nom,
            request.Prenom,
            request.Email,
            string.IsNullOrEmpty(request.Password)
                ? utilisateur.Password 
                : passwordService.HashPassword(utilisateurId, request.Password),
            request.RoleUtilisateurId.HasValue ? new RoleUtilisateurId(request.RoleUtilisateurId.Value) : null,
            request.IsActive,
            new SocieteId(request.SocieteId)
            
        );
    }
}