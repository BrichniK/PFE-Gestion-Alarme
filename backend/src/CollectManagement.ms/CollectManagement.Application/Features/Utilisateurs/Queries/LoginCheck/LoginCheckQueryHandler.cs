using CollectManagement.Application.Contracts.Authentication;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.LoginCheck;

public sealed class LoginCheckQueryHandler
    : IRequestHandler<LoginCheckQuery, AuthenticationResponse>
{
    
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCheckQueryHandler(IUtilisateurRepository utilisateurRepository, IJwtTokenGenerator tokenGenerator)
    {
        _utilisateurRepository = utilisateurRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthenticationResponse> Handle(LoginCheckQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var utilisateurId = new UtilisateurId(request.UtilisateurId);
        
        var utilisateur = await _utilisateurRepository.GetOneAsync(utilisateurId, cancellationToken)
                          ?? throw new BadCredentialException("Utilisateur Invalide");
        
        var token = _tokenGenerator.GenerateToken(utilisateur);

        return new AuthenticationResponse(
            utilisateur.UtilisateurId.Value,
            utilisateur.Nom,
            utilisateur.NomUtilisateur,
            utilisateur.Prenom,
            utilisateur.Email,
            utilisateur.RoleUtilisateur?.Navigations
                .Select(s=> new AuthenticationNavigation(
                    s.NavigationId,
                    s.Actions.Select(a=> (int)a ).ToList(),
                    s.Sections.Select(section=> new AuthenticationSection(
                        section.SectionId,
                        section.Actions.Select(a=> (int)a).ToList()
                    )).ToList()))
                .ToList()??[],
            token,
            utilisateur.SocieteId.Value);
    }
}