using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.SMSConfigurationRepositories;

public class SMSConfigurationRepository : RepositoryBase<SMSConfiguration>, ISMSConfigurationRepository
{
    public SMSConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SMSConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateBulkAsync(SMSConfiguration smsConfiguration, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.SMSConfigurationId.Equals(smsConfiguration.SMSConfigurationId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.ApiUrl, smsConfiguration.ApiUrl)
                    .SetProperty(p => p.IsActive, smsConfiguration.IsActive)
                    .SetProperty(p => p.NombreAlerte, smsConfiguration.NombreAlerte)
                    .SetProperty(p => p.Delai, smsConfiguration.Delai)
                    .SetProperty(p => p.SmsOnAlerte, smsConfiguration.SmsOnAlerte)
                    .SetProperty(p => p.SmsOnBadgeT3, smsConfiguration.SmsOnBadgeT3)
                    .SetProperty(p => p.SmsOnBadgeT4, smsConfiguration.SmsOnBadgeT4)
                    .SetProperty(p => p.SmsOnBadgeT5, smsConfiguration.SmsOnBadgeT5)
                    .SetProperty(p => p.SmsOnTraitement, smsConfiguration.SmsOnTraitement),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
