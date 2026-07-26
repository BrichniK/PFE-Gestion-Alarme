using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.SMS.ValueObjects;

namespace CollectManagement.Application.Features.SMS.Commands.DeleteSMS;

public class DeleteSMSCommandHandler : IRequestHandler<DeleteSMSCommand, bool>
{
    private readonly ISMSRepository _smsRepository;

    public DeleteSMSCommandHandler(ISMSRepository smsRepository)
    {
        _smsRepository = smsRepository;
    }

    public async Task<bool> Handle(DeleteSMSCommand request, CancellationToken cancellationToken)
    {
        var smsId = new SMSId(request.SMSId);

        await _smsRepository
            .DeleteAsync(w => w.SMSId.Equals(smsId), cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
