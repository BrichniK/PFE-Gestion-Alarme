import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { KpiIndicatorsResponse, MaintenanceStatItem, MaintenanceStatResponse } from './stat.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root'
})
export class StatService {
    private _stats: BehaviorSubject<MaintenanceStatItem[] | null> = new BehaviorSubject([]);
    private _statsLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) { }

    get stats$(): Observable<MaintenanceStatItem[]> {
        return this._stats.asObservable();
    }

    get statsLength$(): Observable<number> {
        return this._statsLength.asObservable();
    }

    GetStats(
        page: number = 1,
        size: number = 10,
        search: string = '',
        fromDate?: string | null,
        toDate?: string | null
    ): Observable<MaintenanceStatResponse> {
        const params: Record<string, string | number> = {
            search: search || '',
            page,
            size,
        };

        if (fromDate) {
            params.fromDate = fromDate;
        }

        if (toDate) {
            params.toDate = toDate;
        }

        return this._apiservice
            .Get<MaintenanceStatResponse>('stat/list', {
                params,
            })
            .pipe(
                tap((result) => {
                    this._stats.next(result.data?.stats ?? []);
                    this._statsLength.next(result.data?.length ?? 0);
                }),
                map((r) => r.data)
            );
    }

    GetKpiIndicators(startDate: string, endDate: string, deviceId?: string): Observable<KpiIndicatorsResponse> {
        const params: Record<string, string> = { startDate, endDate };
        if (deviceId) {
            params['deviceId'] = deviceId;
        }

        return this._apiservice
            .Get<KpiIndicatorsResponse>('stat/kpi-indicators', {
                params,
            })
            .pipe(map((r) => r.data));
    }
}
