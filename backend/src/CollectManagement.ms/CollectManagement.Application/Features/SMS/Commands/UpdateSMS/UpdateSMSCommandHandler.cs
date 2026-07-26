using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;

namespace CollectManagement.Application.Features.SMS.Commands.UpdateSMS;

public class UpdateSMSCommandHandler : IRequestHandler<UpdateSMSCommand, bool>
{
    private readonly ISMSRepository _smsRepository;

    public UpdateSMSCommandHandler(ISMSRepository smsRepository)
    {
        _smsRepository = smsRepository;
    }

    public async Task<bool> Handle(UpdateSMSCommand request, CancellationToken cancellationToken)
    {
        var smsId = new SMSId(request.SMSId);
        var sms = await _smsRepository.GetOneAsync(smsId, cancellationToken);

        if (sms == null)
        {
            throw new NotFoundException(nameof(Domain.SMS.SMS), smsId.Value);
        }

        var deviceIds = request.DeviceIds.Select(id => new DeviceId(id)).ToList();
        
        sms.Update(
            request.NomPrenom,
            request.PhoneNumber,
            deviceIds
        );

        await _smsRepository.UpdateBulkAsync(sms, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
