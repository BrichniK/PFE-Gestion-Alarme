using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectManagement.Infrastructure.Persistence.Configurations.ConfigurationGeneraleConfigurations;

public class ConfigurationGeneraleEntityConfiguration : IEntityTypeConfiguration<ConfigurationGenerale>
{
    public void Configure(EntityTypeBuilder<ConfigurationGenerale> builder)
    {
        builder.HasKey(x => x.ConfigurationGeneraleId);

        builder.Property(p => p.ConfigurationGeneraleId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new ConfigurationGeneraleId(new Ulid(value)));

        builder.Property(x => x.EcraserEmployeMaintenance)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AccepterSeulementEmployesPlanifies)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DiagnostiqueObligatoire)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.MonitoringPourcentageSurSommeDurees)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CoefficientGaugeD1)
            .IsRequired()
            .HasDefaultValue(1d);

        builder.Property(x => x.CoefficientGaugeD2)
            .IsRequired()
            .HasDefaultValue(1d);

        builder.Property(x => x.CoefficientGaugeD3)
            .IsRequired()
            .HasDefaultValue(1d);

        builder.Property(x => x.CoefficientGaugeD4)
            .IsRequired()
            .HasDefaultValue(1d);
    }
}
