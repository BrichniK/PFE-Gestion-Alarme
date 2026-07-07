using CollectManagement.Domain.SMSConfigurations;

namespace CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;

public interface ISMSConfigurationRepository : IRepositoryBase<SMSConfiguration>
{
    Task<SMSConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);

    Task UpdateBulkAsync(SMSConfiguration smsConfiguration, CancellationToken cancellationToken);
}
