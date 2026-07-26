import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { ConfigurationGeneraleDto } from './configuration-generale.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root',
})
export class ConfigurationGeneraleService {
    private _config: BehaviorSubject<ConfigurationGeneraleDto | null> =
        new BehaviorSubject<ConfigurationGeneraleDto | null>(null);

    constructor(private _apiservice: ApiService) {}

    get config$(): Observable<ConfigurationGeneraleDto | null> {
        return this._config.asObservable();
    }

    GetConfiguration(): Observable<ConfigurationGeneraleDto> {
        return this._apiservice
            .Get<ConfigurationGeneraleDto>('configuration-generale/get')
            .pipe(
                tap((result) => {
                    this._config.next(result.data);
                }),
                map((r) => r.data)
            );
    }

    UpdateConfiguration(config: {
        ecraserEmployeMaintenance: boolean;
        accepterSeulementEmployesPlanifies: boolean;
        diagnostiqueObligatoire: boolean;
        monitoringPourcentageSurSommeDurees: boolean;
        coefficientGaugeD1: number;
        coefficientGaugeD2: number;
        coefficientGaugeD3: number;
        coefficientGaugeD4: number;
    }): Observable<{ configurationGeneraleId: string }> {
        return this._apiservice
            .Post<{ configurationGeneraleId: string }>(
                'configuration-generale/update',
                config
            )
            .pipe(
                tap((result) => {
                    if (result.success) {
                        this._config.next({
                            configurationGeneraleId:
                                result.data.configurationGeneraleId,
                            ecraserEmployeMaintenance:
                                config.ecraserEmployeMaintenance,
                            accepterSeulementEmployesPlanifies:
                                config.accepterSeulementEmployesPlanifies,
                            diagnostiqueObligatoire:
                                config.diagnostiqueObligatoire,
                            monitoringPourcentageSurSommeDurees:
                                config.monitoringPourcentageSurSommeDurees,
                            coefficientGaugeD1: config.coefficientGaugeD1,
                            coefficientGaugeD2: config.coefficientGaugeD2,
                            coefficientGaugeD3: config.coefficientGaugeD3,
                            coefficientGaugeD4: config.coefficientGaugeD4,
                        });
                    }
                }),
                map((r) => r.data)
            );
    }
}
