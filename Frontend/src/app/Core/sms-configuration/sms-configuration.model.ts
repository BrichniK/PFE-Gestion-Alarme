export interface SMSConfigurationDto {
    smsConfigurationId: string | null;
    apiUrl: string;
    isActive: boolean;
    nombreAlerte: number;
    delai: number;
    smsOnAlerte: boolean;
    smsOnBadgeT3: boolean;
    smsOnBadgeT4: boolean;
    smsOnBadgeT5: boolean;
    smsOnTraitement: boolean;
}
