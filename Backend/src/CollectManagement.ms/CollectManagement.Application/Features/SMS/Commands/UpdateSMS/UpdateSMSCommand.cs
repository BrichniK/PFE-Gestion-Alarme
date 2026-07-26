namespace CollectManagement.Application.Features.SMS.Commands.UpdateSMS;

public record UpdateSMSCommand(
    Ulid SMSId,
    string NomPrenom,
    string PhoneNumber,
    List<Ulid> DeviceIds
) : IRequest<bool>;
