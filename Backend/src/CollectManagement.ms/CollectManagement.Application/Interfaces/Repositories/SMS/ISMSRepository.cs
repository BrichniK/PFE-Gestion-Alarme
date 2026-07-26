using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Application.Interfaces.Repositories.SMS;

public interface ISMSRepository : IRepositoryBase<SMSEntity>
{
    Task<(IReadOnlyList<SMSEntity>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken);
    
    Task<SMSEntity?> GetOneAsync(
        SMSId smsId,
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<SMSEntity>> GetByDeviceIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken);
    
    Task UpdateBulkAsync(SMSEntity sms, CancellationToken cancellationToken);
}
