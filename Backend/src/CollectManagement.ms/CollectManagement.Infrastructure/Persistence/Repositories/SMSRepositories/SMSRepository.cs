using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Infrastructure.Persistence.Repositories.SMSRepositories;

public class SMSRepository : RepositoryBase<SMSEntity>, ISMSRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public SMSRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<(IReadOnlyList<SMSEntity>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet
            .Include(s => s.SMSDevices)
            .ThenInclude(sd => sd.Device)
            .Where(w =>
                string.IsNullOrWhiteSpace(search) ||
                w.NomPrenom.Contains(search) ||
                w.PhoneNumber.Contains(search));
        
        var orderBy = where.OrderByDescending(o => o.NomPrenom);
        
        var prop = TypeDescriptor
            .GetProperties(typeof(SMSEntity))
            .Find(sort ?? "NomPrenom", true);
        
        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<SMSEntity>(o, prop.DisplayName));
        
        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<SMSEntity>(o, prop.DisplayName));
        
        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        
        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        return (readOnlyList, countAsync);
    }
    
    public Task<SMSEntity?> GetOneAsync(SMSId smsId, CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(s => s.SMSDevices)
            .ThenInclude(sd => sd.Device)
            .FirstOrDefaultAsync(s => s.SMSId == smsId, cancellationToken);
    }
    
    public async Task<IReadOnlyList<SMSEntity>> GetByDeviceIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(s => s.SMSDevices)
            .ThenInclude(sd => sd.Device)
            .Where(s => s.SMSDevices.Any(sd => sd.DeviceId == deviceId))
            .ToListAsync(cancellationToken);
    }
    
    public async Task UpdateBulkAsync(SMSEntity sms, CancellationToken cancellationToken)
    {
        // Update SMS properties
        await _dbSet
            .Where(x => x.SMSId == sms.SMSId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.NomPrenom, sms.NomPrenom)
                    .SetProperty(x => x.PhoneNumber, sms.PhoneNumber),
                cancellationToken);
        
        // Handle many-to-many relationship: delete old and add new
        var existingSMS = await _dbSet
            .Include(s => s.SMSDevices)
            .FirstOrDefaultAsync(s => s.SMSId == sms.SMSId, cancellationToken);
        
        if (existingSMS != null)
        {
            // Remove old device relationships
            _dbContext.Set<CollectManagement.Domain.SMS.SMSDevice>()
                .RemoveRange(existingSMS.SMSDevices);
            
            // Add new device relationships
            foreach (var deviceId in sms.SMSDevices.Select(sd => sd.DeviceId))
            {
                var smsDevice = CollectManagement.Domain.SMS.SMSDevice.Create(sms.SMSId, deviceId);
                await _dbContext.Set<CollectManagement.Domain.SMS.SMSDevice>().AddAsync(smsDevice, cancellationToken);
            }
        }
    }
}
