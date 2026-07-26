import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { MonitoringStats } from './monitoring.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root',
})
export class MonitoringService {
    constructor(private _apiservice: ApiService) {}

    GetMonitoringStats(startDate: string, endDate: string, deviceId?: string): Observable<MonitoringStats> {
        const params: any = { startDate, endDate };
        if (deviceId) {
            params.deviceId = deviceId;
        }
        return this._apiservice
            .Get<MonitoringStats>('maintenance/monitoring-stats', { params })
            .pipe(map((r) => r.data));
    }
}
