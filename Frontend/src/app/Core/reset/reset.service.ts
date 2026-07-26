import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root',
})
export class ResetService {
    constructor(private _apiservice: ApiService) {}

    resetDevice(deviceId: string): Observable<boolean> {
        return this._apiservice
            .Post<boolean>(`reset/${deviceId}`, {})
            .pipe(map((r) => r.data));
    }
}
