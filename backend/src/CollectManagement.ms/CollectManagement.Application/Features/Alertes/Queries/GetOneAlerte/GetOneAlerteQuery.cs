namespace CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;

public record GetOneAlerteQuery(Ulid AlerteId) : IRequest<GetOneAlerteResponse>;
