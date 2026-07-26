using CollectManagement.Domain.Common;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;

namespace CollectManagement.Domain.SMSConfigurations;

public class SMSConfiguration : AuditableEntity
{
    public SMSConfigurationId SMSConfigurationId { get; private set; }

    public string ApiUrl { get; private set; }

    public bool IsActive { get; private set; }

    public int NombreAlerte { get; private set; }

    public int Delai { get; private set; }

    public bool SmsOnAlerte { get; private set; }

    public bool SmsOnBadgeT3 { get; private set; }

    public bool SmsOnBadgeT4 { get; private set; }

    public bool SmsOnBadgeT5 { get; private set; }

    public bool SmsOnTraitement { get; private set; }

    private SMSConfiguration(
        SMSConfigurationId smsConfigurationId,
        string apiUrl,
        bool isActive,
        int nombreAlerte,
        int delai,
        bool smsOnAlerte,
        bool smsOnBadgeT3,
        bool smsOnBadgeT4,
        bool smsOnBadgeT5,
        bool smsOnTraitement)
    {
        SMSConfigurationId = smsConfigurationId;
        ApiUrl = apiUrl;
        IsActive = isActive;
        NombreAlerte = nombreAlerte;
        Delai = delai;
        SmsOnAlerte = smsOnAlerte;
        SmsOnBadgeT3 = smsOnBadgeT3;
        SmsOnBadgeT4 = smsOnBadgeT4;
        SmsOnBadgeT5 = smsOnBadgeT5;
        SmsOnTraitement = smsOnTraitement;
    }

    public static SMSConfiguration Create(
        SMSConfigurationId smsConfigurationId,
        string apiUrl,
        bool isActive,
        int nombreAlerte,
        int delai,
        bool smsOnAlerte,
        bool smsOnBadgeT3,
        bool smsOnBadgeT4,
        bool smsOnBadgeT5,
        bool smsOnTraitement)
    {
        return new SMSConfiguration(smsConfigurationId, apiUrl, isActive, nombreAlerte, delai, smsOnAlerte, smsOnBadgeT3, smsOnBadgeT4, smsOnBadgeT5, smsOnTraitement);
    }

    public void Update(string apiUrl, bool isActive, int nombreAlerte, int delai, bool smsOnAlerte, bool smsOnBadgeT3, bool smsOnBadgeT4, bool smsOnBadgeT5, bool smsOnTraitement)
    {
        ApiUrl = apiUrl;
        IsActive = isActive;
        NombreAlerte = nombreAlerte;
        Delai = delai;
        SmsOnAlerte = smsOnAlerte;
        SmsOnBadgeT3 = smsOnBadgeT3;
        SmsOnBadgeT4 = smsOnBadgeT4;
        SmsOnBadgeT5 = smsOnBadgeT5;
        SmsOnTraitement = smsOnTraitement;
    }

    private SMSConfiguration() { }
}
