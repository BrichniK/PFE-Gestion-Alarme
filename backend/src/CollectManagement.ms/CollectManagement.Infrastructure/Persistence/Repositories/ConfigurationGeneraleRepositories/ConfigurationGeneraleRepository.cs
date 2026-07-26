using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.ConfigurationGeneraleRepositories;

public class ConfigurationGeneraleRepository : RepositoryBase<ConfigurationGenerale>, IConfigurationGeneraleRepository
{
    public ConfigurationGeneraleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ConfigurationGenerale?> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateBulkAsync(ConfigurationGenerale configurationGenerale, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.ConfigurationGeneraleId.Equals(configurationGenerale.ConfigurationGeneraleId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.EcraserEmployeMaintenance, configurationGenerale.EcraserEmployeMaintenance)
                    .SetProperty(p => p.AccepterSeulementEmployesPlanifies, configurationGenerale.AccepterSeulementEmployesPlanifies)
                    .SetProperty(p => p.DiagnostiqueObligatoire, configurationGenerale.DiagnostiqueObligatoire)
                    .SetProperty(p => p.MonitoringPourcentageSurSommeDurees, configurationGenerale.MonitoringPourcentageSurSommeDurees)
                    .SetProperty(p => p.CoefficientGaugeD1, configurationGenerale.CoefficientGaugeD1)
                    .SetProperty(p => p.CoefficientGaugeD2, configurationGenerale.CoefficientGaugeD2)
                    .SetProperty(p => p.CoefficientGaugeD3, configurationGenerale.CoefficientGaugeD3)
                    .SetProperty(p => p.CoefficientGaugeD4, configurationGenerale.CoefficientGaugeD4),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
