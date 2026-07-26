namespace CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;

public record GetPagedListAlerteQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListAlerteResponse>;
