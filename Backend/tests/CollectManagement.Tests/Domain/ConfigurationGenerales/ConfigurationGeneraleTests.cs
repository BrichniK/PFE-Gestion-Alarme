using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.ConfigurationGenerales;

public class ConfigurationGeneraleTests
{
    [Fact]
    public void Create_Should_Create_ConfigurationGenerale()
    {
        var id = new ConfigurationGeneraleId(Ulid.NewUlid());

        var config = ConfigurationGenerale.Create(
            id,
            true, false, true, false,
            1.0, 1.5, 2.0, 2.5);

        config.Should().NotBeNull();
        config.ConfigurationGeneraleId.Should().Be(id);
        config.EcraserEmployeMaintenance.Should().BeTrue();
        config.AccepterSeulementEmployesPlanifies.Should().BeFalse();
        config.DiagnostiqueObligatoire.Should().BeTrue();
        config.MonitoringPourcentageSurSommeDurees.Should().BeFalse();
        config.CoefficientGaugeD1.Should().Be(1.0);
        config.CoefficientGaugeD2.Should().Be(1.5);
        config.CoefficientGaugeD3.Should().Be(2.0);
        config.CoefficientGaugeD4.Should().Be(2.5);
    }

    [Fact]
    public void Update_Should_Modify_ConfigurationGenerale()
    {
        var config = ConfigurationGenerale.Create(
            new ConfigurationGeneraleId(Ulid.NewUlid()),
            false, false, false, false,
            1, 1, 1, 1);

        config.Update(true, true, true, true, 3.0, 3.5, 4.0, 4.5);

        config.EcraserEmployeMaintenance.Should().BeTrue();
        config.DiagnostiqueObligatoire.Should().BeTrue();
        config.CoefficientGaugeD1.Should().Be(3.0);
        config.CoefficientGaugeD4.Should().Be(4.5);
    }
}
