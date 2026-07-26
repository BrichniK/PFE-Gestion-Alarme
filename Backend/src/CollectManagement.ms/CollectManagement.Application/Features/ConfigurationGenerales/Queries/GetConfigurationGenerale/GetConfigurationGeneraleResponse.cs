namespace CollectManagement.Application.Features.ConfigurationGenerales.Queries.GetConfigurationGenerale;

public record GetConfigurationGeneraleResponse(
    Ulid? ConfigurationGeneraleId,
    bool EcraserEmployeMaintenance,
    bool AccepterSeulementEmployesPlanifies,
    bool DiagnostiqueObligatoire,
    bool MonitoringPourcentageSurSommeDurees,
    double CoefficientGaugeD1,
    double CoefficientGaugeD2,
    double CoefficientGaugeD3,
    double CoefficientGaugeD4
);
