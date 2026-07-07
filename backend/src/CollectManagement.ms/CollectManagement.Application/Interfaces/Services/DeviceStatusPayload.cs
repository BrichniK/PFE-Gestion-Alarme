namespace CollectManagement.Application.Interfaces.Services;

public record DeviceStatusPayload(
    Ulid DeviceId,
    string? DeviceName,
    string? DeviceMatricule,
    bool IsOnline,
    DateTime LastSeenAt
);
