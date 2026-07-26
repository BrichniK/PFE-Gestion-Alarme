import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { SMSConfigurationDto } from './sms-configuration.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root',
})
export class SMSConfigurationService {
    private _config: BehaviorSubject<SMSConfigurationDto | null> =
        new BehaviorSubject<SMSConfigurationDto | null>(null);

    constructor(private _apiservice: ApiService) {}

    get config$(): Observable<SMSConfigurationDto | null> {
        return this._config.asObservable();
    }

    GetConfiguration(): Observable<SMSConfigurationDto> {
        return this._apiservice
            .Get<SMSConfigurationDto>('sms-configuration/get')
            .pipe(
                tap((result) => {
                    this._config.next(result.data);
                }),
                map((r) => r.data)
            );
    }

    UpdateConfiguration(config: {
        apiUrl: string;
        isActive: boolean;
        nombreAlerte: number;
        delai: number;
        smsOnAlerte: boolean;
        smsOnBadgeT3: boolean;
        smsOnBadgeT4: boolean;
        smsOnBadgeT5: boolean;
        smsOnTraitement: boolean;
    }): Observable<{ smsConfigurationId: string }> {
        return this._apiservice
            .Post<{ smsConfigurationId: string }>(
                'sms-configuration/update',
                config
            )
            .pipe(
                tap((result) => {
                    if (result.success) {
                        this._config.next({
                            smsConfigurationId:
                                result.data.smsConfigurationId,
                            apiUrl: config.apiUrl,
                            isActive: config.isActive,
                            nombreAlerte: config.nombreAlerte,
                            delai: config.delai,
                            smsOnAlerte: config.smsOnAlerte,
                            smsOnBadgeT3: config.smsOnBadgeT3,
                            smsOnBadgeT4: config.smsOnBadgeT4,
                            smsOnBadgeT5: config.smsOnBadgeT5,
                            smsOnTraitement: config.smsOnTraitement,
                        });
                    }
                }),
                map((r) => r.data)
            );
    }
}
