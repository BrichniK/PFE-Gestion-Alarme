using CollectManagement.Domain.ConfigurationGenerales;

namespace CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;

public interface IConfigurationGeneraleRepository : IRepositoryBase<ConfigurationGenerale>
{
    Task<ConfigurationGenerale?> GetConfigurationAsync(CancellationToken cancellationToken);

    Task UpdateBulkAsync(ConfigurationGenerale configurationGenerale, CancellationToken cancellationToken);
}
