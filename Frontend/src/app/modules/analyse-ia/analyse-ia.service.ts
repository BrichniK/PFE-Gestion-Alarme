import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AnalyseIaService {

    private readonly apiUrl =
        'http://localhost:6064/cm/sensor-measurement';

    constructor(
        private readonly http: HttpClient
    ) {}

    getAnalysis(deviceId: string): Observable<any> {
        return this.http.get<any>(
            `${this.apiUrl}/analysis/${deviceId}`
        );
    }
}