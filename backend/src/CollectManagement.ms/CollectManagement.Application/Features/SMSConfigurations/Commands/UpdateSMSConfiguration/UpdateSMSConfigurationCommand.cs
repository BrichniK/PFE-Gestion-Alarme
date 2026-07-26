namespace CollectManagement.Application.Features.SMSConfigurations.Commands.UpdateSMSConfiguration;

public record UpdateSMSConfigurationCommand(
    string ApiUrl,
    bool IsActive,
    int NombreAlerte,
    int Delai,
    bool SmsOnAlerte,
    bool SmsOnBadgeT3,
    bool SmsOnBadgeT4,
    bool SmsOnBadgeT5,
    bool SmsOnTraitement
) : IRequest<UpdateSMSConfigurationResponse>;
