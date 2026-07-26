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

    public LoginQueryHandler(IUtilisateurRepository utilisateurRepository, IPasswordService passwordService,
        IJwtTokenGenerator tokenGenerator)
    {
        _utilisateurRepository = utilisateurRepository;
        _passwordService = passwordService;
        _tokenGenerator = tokenGenerator;
    }


    public async Task<AuthenticationResponse> Handle(
        LoginQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Console.WriteLine("--------------------------------");
        Console.WriteLine($"Login demandé : {request.Login}");

        var utilisateur = await _utilisateurRepository
            .TryToLogin(request.Login, cancellationToken)
            .ConfigureAwait(false);


        if (utilisateur is null)
        {
            Console.WriteLine("❌ Utilisateur introuvable dans la base");
            Console.WriteLine("--------------------------------");

            throw new BadCredentialException("Invalid User");
        }


        Console.WriteLine("✅ Utilisateur trouvé");
        Console.WriteLine($"Id       : {utilisateur.UtilisateurId.Value}");
        Console.WriteLine($"Login DB : {utilisateur.NomUtilisateur}");
        Console.WriteLine($"Email    : {utilisateur.Email}");
        Console.WriteLine($"Active   : {utilisateur.IsActive}");
        Console.WriteLine($"Hash DB  : {utilisateur.Password}");


        var calculatedHash = _passwordService.HashPassword(
            utilisateur.UtilisateurId,
            request.Password);


        Console.WriteLine($"Hash Calculé : {calculatedHash}");


        if (utilisateur.Password != calculatedHash)
        {
            Console.WriteLine("❌ Mot de passe incorrect");
            Console.WriteLine("--------------------------------");

            throw new BadCredentialException("Invalid User");
        }


        Console.WriteLine("✅ Mot de passe valide");
        Console.WriteLine("--------------------------------");


        var token = _tokenGenerator.GenerateToken(utilisateur);


        return new AuthenticationResponse(
            utilisateur.UtilisateurId.Value,
            utilisateur.Nom,
            utilisateur.NomUtilisateur,
            utilisateur.Prenom,
            utilisateur.Email,

            utilisateur.RoleUtilisateur?.Navigations
                .Select(s => new AuthenticationNavigation(
                    s.NavigationId,
                    s.Actions.Select(a => (int)a).ToList(),
                    s.Sections.Select(section => new AuthenticationSection(
                        section.SectionId,
                        section.Actions.Select(a => (int)a).ToList()
                    )).ToList()
                ))
                .ToList() ?? [],

            token,
            utilisateur.SocieteId.Value
        );
    }
}