using CollectManagement.Domain.Common;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;

namespace CollectManagement.Domain.ConfigurationGenerales;

public class ConfigurationGenerale : AuditableEntity
{
    public ConfigurationGeneraleId ConfigurationGeneraleId { get; private set; }

    public bool EcraserEmployeMaintenance { get; private set; }
    public bool AccepterSeulementEmployesPlanifies { get; private set; }
    public bool DiagnostiqueObligatoire { get; private set; }
    public bool MonitoringPourcentageSurSommeDurees { get; private set; }
    public double CoefficientGaugeD1 { get; private set; }
    public double CoefficientGaugeD2 { get; private set; }
    public double CoefficientGaugeD3 { get; private set; }
    public double CoefficientGaugeD4 { get; private set; }

    private ConfigurationGenerale(
        ConfigurationGeneraleId configurationGeneraleId,
        bool ecraserEmployeMaintenance,
        bool accepterSeulementEmployesPlanifies,
        bool diagnostiqueObligatoire,
        bool monitoringPourcentageSurSommeDurees,
        double coefficientGaugeD1,
        double coefficientGaugeD2,
        double coefficientGaugeD3,
        double coefficientGaugeD4)
    {
        ConfigurationGeneraleId = configurationGeneraleId;
        EcraserEmployeMaintenance = ecraserEmployeMaintenance;
        AccepterSeulementEmployesPlanifies = accepterSeulementEmployesPlanifies;
        DiagnostiqueObligatoire = diagnostiqueObligatoire;
        MonitoringPourcentageSurSommeDurees = monitoringPourcentageSurSommeDurees;
        CoefficientGaugeD1 = coefficientGaugeD1;
        CoefficientGaugeD2 = coefficientGaugeD2;
        CoefficientGaugeD3 = coefficientGaugeD3;
        CoefficientGaugeD4 = coefficientGaugeD4;
    }

    public static ConfigurationGenerale Create(
        ConfigurationGeneraleId configurationGeneraleId,
        bool ecraserEmployeMaintenance,
        bool accepterSeulementEmployesPlanifies,
        bool diagnostiqueObligatoire,
        bool monitoringPourcentageSurSommeDurees,
        double coefficientGaugeD1,
        double coefficientGaugeD2,
        double coefficientGaugeD3,
        double coefficientGaugeD4)
    {
        return new ConfigurationGenerale(
            configurationGeneraleId,
            ecraserEmployeMaintenance,
            accepterSeulementEmployesPlanifies,
            diagnostiqueObligatoire,
            monitoringPourcentageSurSommeDurees,
            coefficientGaugeD1,
            coefficientGaugeD2,
            coefficientGaugeD3,
            coefficientGaugeD4);
    }

    public void Update(
        bool ecraserEmployeMaintenance,
        bool accepterSeulementEmployesPlanifies,
        bool diagnostiqueObligatoire,
        bool monitoringPourcentageSurSommeDurees,
        double coefficientGaugeD1,
        double coefficientGaugeD2,
        double coefficientGaugeD3,
        double coefficientGaugeD4)
    {
        EcraserEmployeMaintenance = ecraserEmployeMaintenance;
        AccepterSeulementEmployesPlanifies = accepterSeulementEmployesPlanifies;
        DiagnostiqueObligatoire = diagnostiqueObligatoire;
        MonitoringPourcentageSurSommeDurees = monitoringPourcentageSurSommeDurees;
        CoefficientGaugeD1 = coefficientGaugeD1;
        CoefficientGaugeD2 = coefficientGaugeD2;
        CoefficientGaugeD3 = coefficientGaugeD3;
        CoefficientGaugeD4 = coefficientGaugeD4;
    }

    private ConfigurationGenerale() { }
}
