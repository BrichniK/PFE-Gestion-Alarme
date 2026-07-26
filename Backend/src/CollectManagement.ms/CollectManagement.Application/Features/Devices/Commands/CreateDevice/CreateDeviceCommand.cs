namespace CollectManagement.Application.Features.Devices.Commands.CreateDevice;

public record CreateDeviceCommand(
    string DeviceName,
    string Matricule,
    int NombreCapteur
) : IRequest<CreateDeviceResponse>;
