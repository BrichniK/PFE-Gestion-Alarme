using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Application.Features.SMS.Commands.CreateSMS;

public class CreateSMSCommandHandler : IRequestHandler<CreateSMSCommand, CreateSMSResponse>
{
    private readonly ISMSRepository _smsRepository;

    public CreateSMSCommandHandler(ISMSRepository smsRepository)
    {
        _smsRepository = smsRepository;
    }

    public async Task<CreateSMSResponse> Handle(CreateSMSCommand request, CancellationToken cancellationToken)
    {
        var smsId = new SMSId(Ulid.NewUlid());
        
        var deviceIds = request.DeviceIds.Select(id => new DeviceId(id)).ToList();
        
        var sms = SMSEntity.Create(
            smsId,
            request.NomPrenom,
            request.PhoneNumber,
            deviceIds
        );

        await _smsRepository.AddAsync(sms, cancellationToken).ConfigureAwait(false);

        return new CreateSMSResponse(smsId.Value);
    }
}
