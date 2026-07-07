using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Domain.Devices;

public class Device : AuditableEntity
{
    public DeviceId DeviceId { get; private set; }
    
    public string DeviceName { get; private set; }
    
    public string Matricule { get; private set; }

    public int NombreCapteur { get; private set; }
    
    public bool IsOnline { get; private set; }
    
    public DateTime? LastSeen { get; private set; }
    
    private Device(
        DeviceId deviceId,
        string deviceName,
        string matricule,
        int nombreCapteur
        )
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        Matricule = matricule;
        NombreCapteur = nombreCapteur;
        IsOnline = true;
        LastSeen = DateTime.UtcNow;
    }
    
    public static Device Create(
        DeviceId deviceId,
        string deviceName,
        string matricule,
        int nombreCapteur)
    {
        return new Device(
            deviceId,
            deviceName,
            matricule,
            nombreCapteur);
    }   
    
        public void Update(
            string deviceName,
            string matricule,
            int nombreCapteur
            )
        {
            DeviceName = deviceName;
            Matricule = matricule;
            NombreCapteur = nombreCapteur;
        }
        
        public void SetOnlineStatus(bool isOnline)
        {
            IsOnline = isOnline;
            LastSeen = DateTime.UtcNow;
        }
        
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Device() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
