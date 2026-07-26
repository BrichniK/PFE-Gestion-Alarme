using CollectManagement.Application.Interfaces.Services;

namespace CollectManagement.Application.Features.Societes.Queries.GetLogo;

public class GetLogoQueryHandler
    : IRequestHandler<GetLogoQuery,GetLogoResponse>
{
    private readonly IImageService _imageService;

    public GetLogoQueryHandler(IImageService imageService)
    {
        _imageService = imageService;
    }

    public async Task<GetLogoResponse> Handle(GetLogoQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var logo = await _imageService.GetImage("societe",
                request.LogoName,
                cancellationToken)
            .ConfigureAwait(false);

        return new GetLogoResponse(logo);
    }
}