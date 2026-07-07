using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances.ObjectValues;


namespace CollectManagement.Domain.Maintenances;

public class Maintenance : AuditableEntity
{
    public MaintenanceId MaintenanceId { get; private set; }
    
    public DeviceId DeviceId { get; private set; }
    
    public Device Device { get; private set; }
    
    public EmployeeId EmployeeId { get; private set; }
    
    public Employee Employee { get; private set; }
    
    public DateTime? T1Alerte { get; private set; }
    
    public DateTime? T2Assignment { get; private set; }
    
    public DateTime? T3Arrival { get; private set; }
    
    public DateTime? T4Completion { get; private set; }
    
    public DateTime? T5Confirmation { get; private set; }
    
    public DateTime? T6NextAlert { get; private set; }
    
    public string Description { get; private set; }
    
    private Maintenance(
        MaintenanceId maintenanceId,
        DeviceId deviceId,
        EmployeeId employeeId,
        DateTime? t1Alerte,
        DateTime? t2Assignment,
        DateTime? t3Arrival,
        DateTime? t4Completion,
        DateTime? t5Confirmation,
        DateTime? t6NextAlert,
        string description
        )
    {
        MaintenanceId = maintenanceId;
        DeviceId = deviceId;
        EmployeeId = employeeId;
        T1Alerte = t1Alerte;
        T2Assignment = t2Assignment;
        T3Arrival = t3Arrival;
        T4Completion = t4Completion;
        T5Confirmation = t5Confirmation;
        T6NextAlert = t6NextAlert;
        Description = description;
    }
    
    public static Maintenance Create(
        MaintenanceId maintenanceId,
        DeviceId deviceId,
        EmployeeId employeeId,
        DateTime? t1Alerte,
        DateTime? t2Assignment,
        DateTime? t3Arrival,
        DateTime? t4Completion,
        DateTime? t5Confirmation,
        DateTime? t6NextAlert,
        string description)
    {
        return new Maintenance(
            maintenanceId,
            deviceId,
            employeeId,
            t1Alerte,
            t2Assignment,
            t3Arrival,
            t4Completion,
            t5Confirmation,
            t6NextAlert,
            description);
    }
    
    public void Update(
        DeviceId deviceId,
        EmployeeId employeeId,
        DateTime? t1Alerte,
        DateTime? t2Assignment,
        DateTime? t3Arrival,
        DateTime? t4Completion,
        DateTime? t5Confirmation,
        DateTime? t6NextAlert,
        string description)
    {
        DeviceId = deviceId;
        EmployeeId = employeeId;
        T1Alerte = t1Alerte;
        T2Assignment = t2Assignment;
        T3Arrival = t3Arrival;
        T4Completion = t4Completion;
        T5Confirmation = t5Confirmation;
        T6NextAlert = t6NextAlert;
        Description = description;
    }
    
    /// <summary>
    /// Processes an RFID scan and advances to the next T step.
    /// Returns the step name that was set (T1, T2, T3, T4), or null if all steps are already completed.
    /// </summary>
    public string? ProcessRfidScan()
    {
        var now = DateTime.UtcNow;
        
        if (!T1Alerte.HasValue)
        {
            T1Alerte = now;
            return "T1";
        }
        
        if (!T2Assignment.HasValue)
        {
            T2Assignment = now;
            return "T2";
        }
        
        if (!T3Arrival.HasValue)
        {
            T3Arrival = now;
            return "T3";
        }
        
        if (!T4Completion.HasValue)
        {
            T4Completion = now;
            return "T4";
        }
        
        if (!T5Confirmation.HasValue)
        {
            T5Confirmation = now;
            return "T5";
        }
        
        return null; // All steps already completed
    }
    
    /// <summary>
    /// Returns true if the maintenance is fully completed (all 4 T values are set).
    /// </summary>
    public bool IsCompleted => T1Alerte.HasValue && T2Assignment.HasValue && T3Arrival.HasValue && T4Completion.HasValue && T5Confirmation.HasValue;
    
    /// <summary>
    /// Returns the current step name (the next T to be filled).
    /// </summary>
    public string? CurrentStep
    {
        get
        {
            if (!T1Alerte.HasValue) return "T1";
            if (!T2Assignment.HasValue) return "T2";
            if (!T3Arrival.HasValue) return "T3";
            if (!T4Completion.HasValue) return "T4";
            if (!T5Confirmation.HasValue) return "T5";
            return null;
        }
    }
    
    /// <summary>
    /// Processes an ALARME RFID scan: only updates T3 or T4.
    /// If T3 is null → set T3 to now + 1h. Else if T4 is null → set T4 to now + 1h.
    /// Returns the step name that was set ("T3" or "T4"), or null if both are already completed.
    /// </summary>
    public string? ProcessAlarmRfidScan(bool diagnostiqueObligatoire)
    {
        var now = DateTime.UtcNow.AddHours(1);

        if (!diagnostiqueObligatoire && !T3Arrival.HasValue)
        {
            // Diagnostic optional: first badge jumps directly to repair phase.
            T3Arrival = now;
            T4Completion = now;
            return "T4";
        }
        
        if (!T3Arrival.HasValue)
        {
            T3Arrival = now;
            return "T3";
        }
        
        if (!T4Completion.HasValue)
        {
            T4Completion = now;
            return "T4";
        }
        
        if (!T5Confirmation.HasValue)
        {
            T5Confirmation = now;
            return "T5";
        }
        
        return null; // T3, T4 and T5 already set
    }
    
    /// <summary>
    /// Sets T6NextAlert to indicate a new alert/maintenance was created for the same device.
    /// </summary>
    public void SetT6NextAlert(DateTime t6Value)
    {
        T6NextAlert = t6Value;
    }

    /// <summary>
    /// Reassigns this maintenance to a different employee and resets T3/T4/T5 so the new employee starts fresh.
    /// </summary>
    public void ReassignEmployee(EmployeeId newEmployeeId)
    {
        EmployeeId = newEmployeeId;
        T3Arrival = null;
        T4Completion = null;
        T5Confirmation = null;
    }

    /// <summary>
    /// Auto-completes this maintenance by setting any missing T3/T4/T5 to the given timestamp.
    /// Used when the alert is resolved (A[i]=false) from the device.
    /// </summary>
    public void AutoComplete(DateTime completedAt)
    {
        if (!T3Arrival.HasValue) T3Arrival = completedAt;
        if (!T4Completion.HasValue) T4Completion = completedAt;
        if (!T5Confirmation.HasValue) T5Confirmation = completedAt;
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Maintenance() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    
}