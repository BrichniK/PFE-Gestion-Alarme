using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Domain.Alertes;

public class Alerte : AuditableEntity
{
    public AlerteId AlerteId { get; private set; }
    
    public DateTime? Date { get; private set; }
    
    public DeviceId DispositifId { get; private set; }
    
    public Device Dispositif { get; private set; }
    
    public TypeId TypeId { get; private set; }
    
    public Type Type { get; private set; }
    
    public bool Traiter { get; private set; }
    
    private Alerte(
        AlerteId alerteId,
        DateTime? date,
        DeviceId dispositifId,
        TypeId typeId,
        bool traiter = false
        )
    {
        AlerteId = alerteId;
        Date = date;
        DispositifId = dispositifId;
        TypeId = typeId;
        Traiter = traiter;
    }
    
    public static Alerte Create(
        AlerteId alerteId,
        DateTime? date,
        DeviceId dispositifId,
        TypeId typeId,
        bool traiter = false)
    {
        return new Alerte(
            alerteId,
            date,
            dispositifId,
            typeId,
            traiter);
    }
    
    public void Update(
        DateTime? date,
        DeviceId dispositifId,
        TypeId typeId
        )
    {
        Date = date;
        DispositifId = dispositifId;
        TypeId = typeId;
    }
    
    public void SetTraiter()
    {
        Traiter = true;
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Alerte() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
}