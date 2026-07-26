using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;

namespace CollectManagement.Domain.SMS;

public class SMSDevice : AuditableEntity
{
    public SMSId SMSId { get; private set; }
    
    public SMS SMS { get; private set; }
    
    public DeviceId DeviceId { get; private set; }
    
    public Device Device { get; private set; }
    
    private SMSDevice(
        SMSId smsId,
        DeviceId deviceId)
    {
        SMSId = smsId;
        DeviceId = deviceId;
    }
    
    public static SMSDevice Create(
        SMSId smsId,
        DeviceId deviceId)
    {
        return new SMSDevice(smsId, deviceId);
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private SMSDevice() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
