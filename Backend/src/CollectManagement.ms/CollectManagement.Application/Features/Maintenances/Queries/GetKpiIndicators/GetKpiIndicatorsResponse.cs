namespace CollectManagement.Application.Features.Maintenances.Queries.GetKpiIndicators;

public record GetKpiIndicatorsResponse(
    string Mttr,
    string Mttd,
    string Mttf,
    string Mtbf,
    int NbAlert,
    int NbPannes
);