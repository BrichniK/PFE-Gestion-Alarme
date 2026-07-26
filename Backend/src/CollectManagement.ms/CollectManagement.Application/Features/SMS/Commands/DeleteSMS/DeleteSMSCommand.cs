namespace CollectManagement.Application.Features.SMS.Commands.DeleteSMS;

public record DeleteSMSCommand(Ulid SMSId) : IRequest<bool>;
