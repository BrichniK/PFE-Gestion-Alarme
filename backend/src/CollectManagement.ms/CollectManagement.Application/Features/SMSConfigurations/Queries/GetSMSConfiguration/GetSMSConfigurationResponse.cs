namespace CollectManagement.Application.Features.SMSConfigurations.Queries.GetSMSConfiguration;

public record GetSMSConfigurationResponse(
    Ulid? SMSConfigurationId,
    string ApiUrl,
    bool IsActive,
    int NombreAlerte,
    int Delai,
    bool SmsOnAlerte,
    bool SmsOnBadgeT3,
    bool SmsOnBadgeT4,
    bool SmsOnBadgeT5,
    bool SmsOnTraitement
);
