using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;

namespace CollectManagement.Application.Features.SMSConfigurations.Queries.GetSMSConfiguration;

public class GetSMSConfigurationQueryHandler
    : IRequestHandler<GetSMSConfigurationQuery, GetSMSConfigurationResponse>
{
    private readonly ISMSConfigurationRepository _smsConfigurationRepository;

    public GetSMSConfigurationQueryHandler(ISMSConfigurationRepository smsConfigurationRepository)
    {
        _smsConfigurationRepository = smsConfigurationRepository;
    }

    public async Task<GetSMSConfigurationResponse> Handle(
        GetSMSConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        var config = await _smsConfigurationRepository
            .GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        if (config is null)
        {
            return new GetSMSConfigurationResponse(null, string.Empty, false, 1, 0, true, true, true, true, true);
        }

        return new GetSMSConfigurationResponse(
            config.SMSConfigurationId.Value,
            config.ApiUrl,
            config.IsActive,
            config.NombreAlerte,
            config.Delai,
            config.SmsOnAlerte,
            config.SmsOnBadgeT3,
            config.SmsOnBadgeT4,
            config.SmsOnBadgeT5,
            config.SmsOnTraitement
        );
    }
}
