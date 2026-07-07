namespace CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;

public record GetPagedListAlerteResponse(
    List<GetPagedListAlerteDto> Alertes,
    int Length
);
