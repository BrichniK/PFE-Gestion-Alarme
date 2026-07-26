namespace CollectManagement.Application.Features.Devices.Queries.GetOneDevice;

public record GetOneDeviceResponse(
    Ulid DeviceId,
    string DeviceName,
    string Matricule,
    int NombreCapteur,
    bool IsOnline,
    DateTime? LastSeen
);
