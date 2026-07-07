namespace CollectManagement.Application.Interfaces.Services;

public class MaintenanceRfidResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StepCompleted { get; set; }
    public string? NextStep { get; set; }
    public string? EmployeeNom { get; set; }
    public string? EmployeePrenom { get; set; }
    public string? EmployeeRfid { get; set; }
    public Ulid? MaintenanceId { get; set; }
    public DateTime? T1Alerte { get; set; }
    public DateTime? T2Assignment { get; set; }
    public DateTime? T3Arrival { get; set; }
    public DateTime? T4Completion { get; set; }
}

public interface IMaintenanceRfidService
{
    /// <summary>
    /// Handles an RFID tag scan for the maintenance workflow.
    /// Finds the employee by RFID, then finds their active maintenance record,
    /// and sequentially sets T1 → T2 → T3 → T4.
    /// </summary>
    Task<MaintenanceRfidResponse> HandleRfidScanAsync(string rfid, CancellationToken cancellationToken);
}
