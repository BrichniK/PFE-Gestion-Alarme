namespace CollectManagement.Application.Interfaces.Services;

public record MaintenanceCaptureRealtimePayload(
    Ulid CaptureHistoryId,
    Ulid MaintenanceId,
    Ulid DeviceId,
    string? DeviceName,
    string? DeviceMatricule,
    Ulid EmployeeId,
    string? EmployeeNom,
    string? EmployeePrenom,
    string TagId,
    string Step,
    string Status,
    DateTime CapturedAt,
    DateTime? T3Arrival,
    DateTime? T4Completion,
    DateTime? T5Confirmation
);
