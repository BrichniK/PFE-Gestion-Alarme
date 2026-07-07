namespace CollectManagement.Application.Features.Devices.Commands.UpdateDevice;

public record UpdateDeviceCommand(
    Ulid DeviceId,
    string DeviceName,
    string Matricule,
    int NombreCapteur
) : IRequest;
