using CollectManagement.Domain.Common;
using CollectManagement.Domain.SMS.ValueObjects;

namespace CollectManagement.Domain.SMS;

public class SMS : AuditableEntity
{
    public SMSId SMSId { get; private set; }
    
    public string NomPrenom { get; private set; }
    
    public string PhoneNumber { get; private set; }
    
    public ICollection<SMSDevice> SMSDevices { get; private set; } = new List<SMSDevice>();
    
    private SMS(
        SMSId smsId,
        string nomPrenom,
        string phoneNumber,
        IEnumerable<Devices.ValueObjects.DeviceId> deviceIds)
    {
        SMSId = smsId;
        NomPrenom = nomPrenom;
        PhoneNumber = phoneNumber;
        SetDeviceRelations(deviceIds);
    }
    
    public static SMS Create(
        SMSId smsId,
        string nomPrenom,
        string phoneNumber,
        IEnumerable<Devices.ValueObjects.DeviceId> deviceIds)
    {
        return new SMS(
            smsId,
            nomPrenom,
            phoneNumber,
            deviceIds);
    }
    
    public void Update(
        string nomPrenom,
        string phoneNumber,
        IEnumerable<Devices.ValueObjects.DeviceId> deviceIds)
    {
        NomPrenom = nomPrenom;
        PhoneNumber = phoneNumber;
        SetDeviceRelations(deviceIds);
    }
    
    private void SetDeviceRelations(IEnumerable<Devices.ValueObjects.DeviceId> deviceIds)
    {
        SMSDevices = deviceIds
            .Distinct()
            .Select(deviceId => SMSDevice.Create(SMSId, deviceId))
            .ToList();
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private SMS() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
