using CollectManagement.Application.Interfaces.Repositories.SMS;

namespace CollectManagement.Application.Features.SMS.Queries.GetSMSList;

public class GetSMSListQueryHandler : IRequestHandler<GetSMSListQuery, GetSMSListResponse>
{
    private readonly ISMSRepository _smsRepository;

    public GetSMSListQueryHandler(ISMSRepository smsRepository)
    {
        _smsRepository = smsRepository;
    }

    public async Task<GetSMSListResponse> Handle(GetSMSListQuery request, CancellationToken cancellationToken)
    {
        var (smsList, length) = await _smsRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken)
            .ConfigureAwait(false);

        return new GetSMSListResponse(
            smsList.Select(s => new DTOs.SMSDto(
                s.SMSId.Value,
                s.NomPrenom,
                s.PhoneNumber,
                s.SMSDevices.Select(sd => new DTOs.DeviceDto(
                    sd.Device.DeviceId.Value,
                    sd.Device.DeviceName,
                    sd.Device.Matricule
                )).ToList()
            )).ToList(),
            length);
    }
}
