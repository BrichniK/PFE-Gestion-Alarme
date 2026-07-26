namespace CollectManagement.Application.Features.Societes.Queries.GetLogo;

public record GetLogoQuery(
    string LogoName
    ):IRequest<GetLogoResponse>;