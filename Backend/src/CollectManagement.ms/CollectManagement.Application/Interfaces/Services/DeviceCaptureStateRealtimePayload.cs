namespace CollectManagement.Application.Interfaces.Services;

public record DeviceCaptureStateRealtimePayload(
    Ulid DeviceId,
    string? DeviceName,
    string? DeviceMatricule,
    int TotalCaptures,
    int WorkingCaptures,
    string Capture1Status,
    string Capture2Status,
    DateTime? Capture1LastErrorAt,
    DateTime? Capture2LastErrorAt,
    IReadOnlyList<string> CaptureStatuses,
    IReadOnlyList<DateTime?> CaptureLastErrorAt,
    IReadOnlyList<string?> CaptureAlertLabels,
    int? MaintenanceCaptureIndex,
    bool IsUnderMaintenance,
    string? MaintenancePhase,
    DateTime? MaintenancePhaseStartedAt,
    DateTime? MaintenanceStartedAt,
    DateTime? MaintenanceFinishedAt,
    string? MaintenanceEmployeeName,
    DateTime LastUpdatedAt,
    string Trigger
);
