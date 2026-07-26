using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Enums;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Societes.Commands.CreateSociete;

public class CreateSocieteCommandHandler
    :IRequestHandler<CreateSocieteCommand,CreateSocieteReponse>
{
    private readonly ISocieteRepository _societeRepository;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IPasswordService _passwordService;

    public CreateSocieteCommandHandler(
        ISocieteRepository societeRepository,
        IMapper mapper, IImageService imageService, IUtilisateurRepository utilisateurRepository, IPasswordService passwordService)
    {
        _societeRepository = societeRepository;
        _mapper = mapper;
        _imageService = imageService;
        _utilisateurRepository = utilisateurRepository;
        _passwordService = passwordService;
    }

    public async Task<CreateSocieteReponse> Handle(CreateSocieteCommand request, CancellationToken cancellationToken)
    {
        string? imageName = null;
        var societeId = new SocieteId(Ulid.NewUlid());
        
        if (request.LogoData is not null)
        {
            imageName = $"{societeId}.{request.LogoExtension}";
            await _imageService.SaveImage(
                request.LogoData,
                "societe",
                imageName,
                cancellationToken
            ).ConfigureAwait(false);
        }
        
        
        var societe = Societe.Create(
               societeId,
        imageName,
        request.Nom,
        request.MatriculeFiscal,
        request.Rne,
        request.Capital,
        request.DateOverture,
        request.Telephone1,
        request.Telephone2,
        request.Fax1,
        request.Fax2,
        request.Email,
        request.Adresse,
        request.CodeSociete
        );

        await _societeRepository
            .AddAsync(societe, cancellationToken)
            .ConfigureAwait(false);
        
        // --- Création de l'utilisateur par défaut ---
        var utilisateurId = new UtilisateurId(Ulid.NewUlid());
        var nomUtilisateur = "USER" + request.Nom;
        var motDePasse = "USER" + request.Nom;

        // var utilisateur = Utilisateur.Create(
        //     utilisateurId,
        //     nomUtilisateur,         
        //     request.Nom,              
        //     request.Nom,            
        //     request.Email ,    
        //     _passwordService.HashPassword(utilisateurId, motDePasse),
        //     UtilisateurRole.Admin,     
        //     societeId                
        // );
        //
        // await _utilisateurRepository
        //     .AddAsync(utilisateur, cancellationToken)
        //     .ConfigureAwait(false);

        return _mapper.Map<CreateSocieteReponse>(societe);

    }


}