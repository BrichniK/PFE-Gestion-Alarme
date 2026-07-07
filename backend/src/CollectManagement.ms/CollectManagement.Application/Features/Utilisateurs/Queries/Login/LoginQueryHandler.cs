using CollectManagement.Application.Common;
using CollectManagement.Application.Contracts.Authentication;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.Login;

public sealed class LoginQueryHandler
    : IRequestHandler<LoginQuery, AuthenticationResponse>
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginQueryHandler(IUtilisateurRepository utilisateurRepository, IPasswordService passwordService, IJwtTokenGenerator tokenGenerator)
    {
        _utilisateurRepository = utilisateurRepository;
        _passwordService = passwordService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthenticationResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var utilisateur = await _utilisateurRepository.TryToLogin(
            request.Login,
            cancellationToken).ConfigureAwait(false);
        
        if(utilisateur is null || _passwordService.VerifyHashedPassword(
               utilisateur.UtilisateurId,
               utilisateur.Password,
               request.Password) == PasswordVerificationResult.Failure)
            throw new BadCredentialException("Invalid User");

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