using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Utilisateurs.Commands.CreateUtilisateur;

public class CreateUtilisateurCommandHandler
    : IRequestHandler<CreateUtilisateurCommand, CreateUtilisateurResponse>
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IPasswordService _passwordService;
    private readonly ISocieteRepository _societeRepository; // Pour vérifier la société

    public CreateUtilisateurCommandHandler(IUtilisateurRepository utilisateurRepository, IPasswordService passwordService, ISocieteRepository societeRepository)
    {
        _utilisateurRepository = utilisateurRepository;
        _passwordService = passwordService;
        _societeRepository = societeRepository;
    }

    public async Task<CreateUtilisateurResponse> Handle(CreateUtilisateurCommand request, CancellationToken cancellationToken)
    {
        var utilisateurId = new UtilisateurId(Ulid.NewUlid());
        // var requsetRole = (UtilisateurRole)request.Role;
        
        var utilisateur = Utilisateur.Create(
            utilisateurId,
            request.NomUtilisateur,
            request.Nom,
            request.Prenom,
            request.Email,
            _passwordService.HashPassword(
                utilisateurId, 
                request.Password),
            request.RoleUtilisateurId.HasValue ? new RoleUtilisateurId(request.RoleUtilisateurId.Value) : null,
            request.IsActive,
            new SocieteId(request.SocieteId)

        );

        await _utilisateurRepository
            .AddAsync(utilisateur, cancellationToken)
            .ConfigureAwait(false);

        return new CreateUtilisateurResponse(utilisateurId.Value);

    }
}