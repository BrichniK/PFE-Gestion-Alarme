import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { SettingConfig } from './setting-config.model';

@Injectable({
    providedIn: 'root'
})
export class SettingConfigService {
    private httpClient = inject(HttpClient);
    private _settingConfig: SettingConfig | null = null;

    async loadConfig() {

        try {
            this._settingConfig = await lastValueFrom(this.httpClient.get<SettingConfig>('./config/setting-config.json'));
        } catch (error) {
            console.error('Error loading config file', error);
        }

        // return this.httpClient.get<SettingConfig>('./config/setting-config.json')
        //     .pipe(tap(result=> {
        //         this._settingConfig = result;
        //         console.log(result);
        //     }));
    }

    get baseApi():string{
        return this._settingConfig?.baseApi;
    }
}
