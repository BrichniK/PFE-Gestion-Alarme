using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;

namespace CollectManagement.Application.Features.SMSConfigurations.Commands.UpdateSMSConfiguration;

public class UpdateSMSConfigurationCommandHandler
    : IRequestHandler<UpdateSMSConfigurationCommand, UpdateSMSConfigurationResponse>
{
    private readonly ISMSConfigurationRepository _smsConfigurationRepository;

    public UpdateSMSConfigurationCommandHandler(ISMSConfigurationRepository smsConfigurationRepository)
    {
        _smsConfigurationRepository = smsConfigurationRepository;
    }

    public async Task<UpdateSMSConfigurationResponse> Handle(
        UpdateSMSConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _smsConfigurationRepository
            .GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            // First time — create the configuration row
            var newId = new SMSConfigurationId(Ulid.NewUlid());
            var config = SMSConfiguration.Create(newId, request.ApiUrl, request.IsActive, request.NombreAlerte, request.Delai, request.SmsOnAlerte, request.SmsOnBadgeT3, request.SmsOnBadgeT4, request.SmsOnBadgeT5, request.SmsOnTraitement);

            await _smsConfigurationRepository
                .AddAsync(config, cancellationToken)
                .ConfigureAwait(false);

            return new UpdateSMSConfigurationResponse(newId.Value);
        }

        // Update existing row
        existing.Update(request.ApiUrl, request.IsActive, request.NombreAlerte, request.Delai, request.SmsOnAlerte, request.SmsOnBadgeT3, request.SmsOnBadgeT4, request.SmsOnBadgeT5, request.SmsOnTraitement);
        await _smsConfigurationRepository
            .UpdateBulkAsync(existing, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateSMSConfigurationResponse(existing.SMSConfigurationId.Value);
    }
}
