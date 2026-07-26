namespace CollectManagement.Application.Features.SMS.Commands.CreateSMS;

public record CreateSMSCommand(
    string NomPrenom,
    string PhoneNumber,
    List<Ulid> DeviceIds
) : IRequest<CreateSMSResponse>;
