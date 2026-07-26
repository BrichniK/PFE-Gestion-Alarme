namespace CollectManagement.Application.Features.Maintenances.Queries.GetKpiIndicators;

public record GetKpiIndicatorsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    Ulid? DeviceId
) : IRequest<GetKpiIndicatorsResponse>;