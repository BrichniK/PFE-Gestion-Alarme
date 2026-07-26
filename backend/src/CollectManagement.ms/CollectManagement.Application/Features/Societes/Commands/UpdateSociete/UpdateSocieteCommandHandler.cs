using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Societes.Commands.UpdateSociete;

public class UpdateSocieteCommandHandler
    :IRequestHandler<UpdateSocieteCommand>
{
    private readonly ISocieteRepository _societeRepository;
    private readonly IImageService _imageService;

    public UpdateSocieteCommandHandler(
        ISocieteRepository societeRepository, 
        IImageService imageService)
    {
        _societeRepository = societeRepository;
        _imageService = imageService;
    }

    public async Task Handle(UpdateSocieteCommand request, CancellationToken cancellationToken)
    {
        var societeId = new SocieteId(request.SocieteId);
        
        // Vérifier si une nouvelle image est fournie
        string? newImageName = request.LogoPath; // Garder l'ancien logo par défaut

        if (!string.IsNullOrEmpty(request.LogoData) && !string.IsNullOrEmpty(request.LogoExtension))
        {
            newImageName = $"{request.SocieteId}.{request.LogoExtension}";

            // Enregistrer la nouvelle image
            await _imageService.SaveImage(
                request.LogoData,
                "societe",
                newImageName,
                cancellationToken
            ).ConfigureAwait(false);
        }

        var societe = Societe.Create(
            societeId,
            newImageName,
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
        await _societeRepository.UpdateBulkAsync(societe, cancellationToken)
            .ConfigureAwait(false);
        
    }
}