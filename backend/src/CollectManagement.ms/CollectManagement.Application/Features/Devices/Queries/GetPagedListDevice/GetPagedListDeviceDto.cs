namespace CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;

public record GetPagedListDeviceDto(
    Ulid DeviceId,
    string DeviceName,
    string Matricule,
    int NombreCapteur,
    bool IsOnline,
    DateTime? LastSeen
);
