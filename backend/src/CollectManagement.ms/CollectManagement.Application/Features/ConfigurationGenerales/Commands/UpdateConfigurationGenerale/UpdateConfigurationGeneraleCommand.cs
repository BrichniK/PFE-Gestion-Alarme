namespace CollectManagement.Application.Features.ConfigurationGenerales.Commands.UpdateConfigurationGenerale;

public record UpdateConfigurationGeneraleCommand(
    bool EcraserEmployeMaintenance,
    bool AccepterSeulementEmployesPlanifies,
    bool DiagnostiqueObligatoire,
    bool MonitoringPourcentageSurSommeDurees,
    double CoefficientGaugeD1,
    double CoefficientGaugeD2,
    double CoefficientGaugeD3,
    double CoefficientGaugeD4
) : IRequest<UpdateConfigurationGeneraleResponse>;
