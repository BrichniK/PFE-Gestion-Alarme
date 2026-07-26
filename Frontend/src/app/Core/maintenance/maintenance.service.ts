import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import {
    DeviceCaptureHistoryItem,
    Maintenance,
    MaintenanceRfidResponse,
    PagedMaintenance,
} from './maintenance.model';
import { ApiService } from '../common/api.service';

interface CreateMaintenanceApiResponse {
    maintenanceId: string;
}

@Injectable({
    providedIn: 'root'
})
export class MaintenanceService {
    private _maintenances: BehaviorSubject<Maintenance[] | null> = new BehaviorSubject([]);
    private _maintenance: BehaviorSubject<Maintenance | null> = new BehaviorSubject(null);
    private _maintenancesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);
    private _captureHistory: BehaviorSubject<DeviceCaptureHistoryItem[] | null> = new BehaviorSubject([]);

    constructor(private _apiservice: ApiService) { }

    get maintenances$(): Observable<Maintenance[]> {
        return this._maintenances.asObservable();
    }

    get maintenance$(): Observable<Maintenance> {
        return this._maintenance.asObservable();
    }

    get maintenancesLength$(): Observable<number> {
        return this._maintenancesLength.asObservable();
    }

    get captureHistory$(): Observable<DeviceCaptureHistoryItem[]> {
        return this._captureHistory.asObservable();
    }

    GetMaintenance(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = '',
        filter: string = 'all',
        fromDate?: string | null,
        toDate?: string | null
    ): Observable<PagedMaintenance> {
        const params: Record<string, string | number> = {
            search: search || '',
            sort,
            order,
            page,
            size,
            filter,
        };

        if (fromDate) {
            params.fromDate = fromDate;
        }

        if (toDate) {
            params.toDate = toDate;
        }

        return this._apiservice
            .Get<PagedMaintenance>('maintenance/list', {
                params,
            })
            .pipe(
                tap((result) => {
                    this._maintenances.next(result.data?.maintenances ?? []);
                    this._maintenancesLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewMaintenance(): Observable<Maintenance> {
        const newMaintenance: Maintenance = {
            maintenanceId: 'new',
            deviceId: null,
            employeeId: null,
            t1Alerte: null,
            t2Assignment: null,
            t3Arrival: null,
            t4Completion: null,
            t6NextAlert: null,
            description: null,
        };
        this._maintenances.next([newMaintenance, ...this._maintenances.value]);
        return of(newMaintenance);
    }

    AddMaintenance(maintenance: Maintenance): Observable<Maintenance> {
        const { maintenanceId, ...body } = maintenance;
        return this._apiservice.Post<CreateMaintenanceApiResponse>('maintenance/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newMaintenance: Maintenance = {
                    ...body,
                    maintenanceId: r.data?.maintenanceId,
                };
                this._maintenances.next([newMaintenance, ...this._maintenances.value ?? []]);
                return newMaintenance;
            })
        );
    }

    UpdateMaintenance(maintenance: Maintenance): Observable<boolean> {
        return this._apiservice.Patch<boolean>('maintenance/update', maintenance).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._maintenances.value?.findIndex(
                    (item) => item.maintenanceId === maintenance.maintenanceId
                ) ?? -1;
                if (index !== -1) {
                    this._maintenances.value[index] = maintenance;
                    this._maintenances.next(this._maintenances.value);
                }
                return true;
            })
        );
    }

    DeleteMaintenance(maintenance: { maintenanceId: string }): Observable<boolean> {
        if (maintenance.maintenanceId === 'new') {
            const index = this._maintenances.value.findIndex(
                (item) => item.maintenanceId === maintenance.maintenanceId
            );
            this._maintenances.value.splice(index, 1);
            this._maintenances.next(this._maintenances.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `maintenance/${maintenance.maintenanceId}/delete?maintenanceId=${maintenance.maintenanceId}`,
                maintenance
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._maintenances.value.findIndex(
                        (item) => item.maintenanceId === maintenance.maintenanceId
                    ) ?? -1;
                    if (index !== -1) {
                        this._maintenances.value.splice(index, 1);
                        this._maintenances.next(this._maintenances.value);
                    }
                    return true;
                })
            );
    }

    GetMaintenanceById(id: string): Observable<Maintenance> {
        const localMaintenance = this._maintenances.value?.find((x) => x.maintenanceId === id);
        if (localMaintenance) {
            return of(localMaintenance);
        }

        return this._apiservice.Get<Maintenance>(`maintenance/${id}/one`).pipe(
            map((r) => r.data),
            tap((maintenance) => {
                if (!maintenance) {
                    return;
                }
                const current = this._maintenances.value ?? [];
                const exists = current.some((item) => item.maintenanceId === maintenance.maintenanceId);
                if (!exists) {
                    this._maintenances.next([maintenance, ...current]);
                }
            })
        );
    }

    ScanRfid(rfid: string): Observable<MaintenanceRfidResponse> {
        return this._apiservice.Post2<MaintenanceRfidResponse>('maintenance/scan-rfid', { rfid }).pipe(
            map((r) => r.data)
        );
    }

    GetDeviceCaptureHistory(
        deviceId: string,
        page: number = 1,
        size: number = 50
    ): Observable<DeviceCaptureHistoryItem[]> {
        return this._apiservice
            .Get<any>(`maintenance/${deviceId}/capture-history`, {
                params: { page, size },
            })
            .pipe(
                map((r) => this.normalizeCaptureHistoryList(r.data)),
                tap((history) => this._captureHistory.next(history))
            );
    }

    private normalizeCaptureHistoryList(data: any): DeviceCaptureHistoryItem[] {
        if (!data) {
            return [];
        }

        const candidates = [
            data.items,
            data.history,
            data.captureHistory,
            data.deviceCaptureHistory,
            data.maintenances,
            data.list,
        ];

        const list = Array.isArray(data)
            ? data
            : candidates.find((candidate) => Array.isArray(candidate)) ?? [];

        return list.map((item) => this.normalizeCaptureHistoryItem(item));
    }

    private normalizeCaptureHistoryItem(data: any): DeviceCaptureHistoryItem {
        return {
            maintenanceId: data?.maintenanceId ?? null,
            deviceId: data?.deviceId ?? '',
            deviceName: data?.deviceName ?? null,
            deviceMatricule: data?.deviceMatricule ?? null,
            employeeId: data?.employeeId ?? null,
            employeeNom: data?.employeeNom ?? null,
            employeePrenom: data?.employeePrenom ?? null,
            tagRfid: data?.tagRfid ?? null,
            capture1Status: data?.capture1Status ?? null,
            capture2Status: data?.capture2Status ?? null,
            status: data?.status ?? null,
            maintenanceStartedAt: data?.maintenanceStartedAt ?? null,
            maintenanceFinishedAt: data?.maintenanceFinishedAt ?? null,
            lastUpdatedAt: data?.lastUpdatedAt ?? null,
        };
    }
}
