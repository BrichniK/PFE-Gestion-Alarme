namespace CollectManagement.Application.Features.SMS.Queries.GetSMSList.DTOs;

public record SMSDto(
    Ulid SMSId,
    string NomPrenom,
    string PhoneNumber,
    List<DeviceDto> Devices
);

public record DeviceDto(
    Ulid DeviceId,
    string DeviceName,
    string Matricule
);
