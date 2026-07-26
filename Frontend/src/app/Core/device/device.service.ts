import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import {
    Device,
    DeviceCaptureState,
    DeviceCaptureStateRealtimePayload,
    DeviceStatusPayload,
    PagedDevice,
} from './device.model';
import { ApiService } from '../common/api.service';

interface CreateDeviceApiResponse {
    deviceId: string;
}

@Injectable({
    providedIn: 'root'
})
export class DeviceService {
    private _devices: BehaviorSubject<Device[] | null> = new BehaviorSubject([]);
    private _device: BehaviorSubject<Device | null> = new BehaviorSubject(null);
    private _devicesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);
    private _captureStates: BehaviorSubject<DeviceCaptureState[] | null> = new BehaviorSubject([]);

    constructor(private _apiservice: ApiService) {}

    get devices$(): Observable<Device[]> {
        return this._devices.asObservable();
    }

    get device$(): Observable<Device> {
        return this._device.asObservable();
    }

    get devicesLength$(): Observable<number> {
        return this._devicesLength.asObservable();
    }

    get captureStates$(): Observable<DeviceCaptureState[]> {
        return this._captureStates.asObservable();
    }

    GetDevice(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedDevice> {
        return this._apiservice
            .Get<PagedDevice>('device/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._devices.next(result.data?.devices ?? []);
                    this._devicesLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewDevice(): Observable<Device> {
        const newDevice: Device = {
            deviceId: 'new',
            deviceName: null,
            matricule: null,
            nombreCapteur: 0,
        };
        return of(newDevice);
    }

    AddDevice(device: Device): Observable<Device> {
        const { deviceId, ...body } = device;
        return this._apiservice.Post<CreateDeviceApiResponse>('device/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newDevice: Device = {
                    ...body,
                    deviceId: r.data?.deviceId,
                };
                this._devices.next([newDevice, ...this._devices.value ?? []]);
                return newDevice;
            })
        );
    }

    UpdateDevice(device: Device): Observable<boolean> {
        return this._apiservice.Patch<boolean>('device/update', device).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._devices.value?.findIndex(
                    (item) => item.deviceId === device.deviceId
                ) ?? -1;
                if (index !== -1) {
                    this._devices.value[index] = device;
                    this._devices.next(this._devices.value);
                }
                return true;
            })
        );
    }

    DeleteDevice(device: { deviceId: string }): Observable<boolean> {
        if (device.deviceId === 'new') {
            const index = this._devices.value.findIndex(
                (item) => item.deviceId === device.deviceId
            );
            this._devices.value.splice(index, 1);
            this._devices.next(this._devices.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `device/${device.deviceId}/delete?deviceId=${device.deviceId}`,
                device
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._devices.value.findIndex(
                        (item) => item.deviceId === device.deviceId
                    ) ?? -1;
                    if (index !== -1) {
                        this._devices.value.splice(index, 1);
                        this._devices.next(this._devices.value);
                    }
                    return true;
                })
            );
    }

    GetDeviceById(id: string): Observable<Device> {
        const localDevice = this._devices.value?.find((x) => x.deviceId === id);
        if (localDevice) {
            return of(localDevice);
        }

        return this._apiservice.Get<Device>(`device/${id}/one`).pipe(
            map((r) => r.data),
            tap((device) => {
                if (!device) {
                    return;
                }
                const current = this._devices.value ?? [];
                const exists = current.some((item) => item.deviceId === device.deviceId);
                if (!exists) {
                    this._devices.next([device, ...current]);
                }
            })
        );
    }

    GetDeviceCaptureStateById(deviceId: string): Observable<DeviceCaptureState> {
        return this._apiservice.Get<any>(`device/${deviceId}/capture-state`).pipe(
            map((r) => this.normalizeCaptureState(this.extractSingleState(r.data)))
        );
    }

    GetDeviceCaptureStateList(): Observable<DeviceCaptureState[]> {
        return this._apiservice.Get<any>('device/capture-state/list').pipe(
            map((r) => this.normalizeCaptureStateList(r.data)),
            tap((states) => this._captureStates.next(states))
        );
    }

    applyRealtimeCaptureState(payload: DeviceCaptureStateRealtimePayload): void {
        const state = this.normalizeCaptureState(payload);
        const current = [...(this._captureStates.value ?? [])];
        const index = current.findIndex((item) => item.deviceId === state.deviceId);
        if (index === -1) {
            current.unshift(state);
        } else {
            current[index] = { ...current[index], ...state };
        }
        this._captureStates.next(current);
    }

    applyDeviceStatusChange(payload: DeviceStatusPayload): void {
        const current = [...(this._devices.value ?? [])];
        const index = current.findIndex((item) => item.deviceId === payload.deviceId);
        if (index !== -1) {
            current[index] = {
                ...current[index],
                isOnline: payload.isOnline,
                lastSeen: payload.lastSeenAt,
            };
            this._devices.next(current);
        }
    }

    private extractSingleState(data: any): any {
        if (!data) {
            return data;
        }

        return data.captureState ?? data.deviceCaptureState ?? data.item ?? data;
    }

    private normalizeCaptureStateList(data: any): DeviceCaptureState[] {
        if (!data) {
            return [];
        }

        const candidates = [
            data.captureStates,
            data.deviceCaptureStates,
            data.items,
            data.devices,
            data.list,
        ];

        const list = Array.isArray(data)
            ? data
            : candidates.find((candidate) => Array.isArray(candidate)) ?? [];

        return list.map((item) => this.normalizeCaptureState(item));
    }

    private normalizeCaptureState(data: any): DeviceCaptureState {
        const mqttAlarmFlags: boolean[] = Array.isArray(data?.A)
            ? data.A.map((value: unknown) => value === true)
            : [];

        const hasTotalCaptures =
            (data?.totalCaptures !== undefined && data?.totalCaptures !== null)
            || mqttAlarmFlags.length > 0;
        const totalCapturesInput = Number(data?.totalCaptures ?? mqttAlarmFlags.length ?? 0);
        const totalCaptures = Number.isFinite(totalCapturesInput)
            ? Math.max(0, Math.floor(totalCapturesInput))
            : 0;

        const mqttCaptureStatuses = mqttAlarmFlags.map((isInAlert) =>
            isInAlert ? 'ERROR' : 'WORKING'
        );
        const rawCaptureStatuses = Array.isArray(data?.captureStatuses)
            ? data.captureStatuses
            : mqttCaptureStatuses;
        const normalizedRawStatuses = rawCaptureStatuses
            .map((status: unknown) =>
                this.isCaptureStatus(status) ? status : 'NOT_AVAILABLE'
            );

        const fallbackStatuses = [
            this.isCaptureStatus(data?.capture1Status) ? data.capture1Status : 'NOT_AVAILABLE',
            this.isCaptureStatus(data?.capture2Status) ? data.capture2Status : 'NOT_AVAILABLE',
        ];

        const baseStatuses = normalizedRawStatuses.length > 0
            ? normalizedRawStatuses
            : fallbackStatuses;
        const resolvedTotalCaptures = hasTotalCaptures ? totalCaptures : baseStatuses.length;
        const captureStatuses = Array.from({ length: resolvedTotalCaptures }, (_, index) =>
            baseStatuses[index] ?? 'NOT_AVAILABLE'
        );

        const rawCaptureLastErrorAt = Array.isArray(data?.captureLastErrorAt)
            ? data.captureLastErrorAt
            : [];
        const mqttTiSecondsInput = Number(data?.TI);
        const mqttTiSeconds = Number.isFinite(mqttTiSecondsInput)
            ? mqttTiSecondsInput
            : null;
        const mqttDurations: number[] = Array.isArray(data?.Dur)
            ? data.Dur.map((value: unknown) => Number(value))
            : [];
        const mqttCaptureLastErrorAt = Array.from(
            { length: resolvedTotalCaptures },
            (_, index) => {
                const status = captureStatuses[index] ?? 'NOT_AVAILABLE';
                const durationSeconds = mqttDurations[index];

                if (status !== 'ERROR') {
                    return null;
                }

                if (mqttTiSeconds === null || !Number.isFinite(durationSeconds)) {
                    return null;
                }

                const startMs = Math.floor((mqttTiSeconds - durationSeconds) * 1000);
                if (!Number.isFinite(startMs) || startMs <= 0) {
                    return null;
                }

                return new Date(startMs).toISOString();
            }
        );
        const fallbackLastErrorAt = [
            data?.capture1LastErrorAt ?? null,
            data?.capture2LastErrorAt ?? null,
        ];
        const baseLastErrorAt = rawCaptureLastErrorAt.length > 0
            ? rawCaptureLastErrorAt
            : mqttCaptureLastErrorAt.some((value) => typeof value === 'string')
                ? mqttCaptureLastErrorAt
            : fallbackLastErrorAt;
        const captureLastErrorAt = Array.from(
            { length: resolvedTotalCaptures },
            (_, index) => baseLastErrorAt[index] ?? null
        );

        const rawCaptureAlertLabels = Array.isArray(data?.captureAlertLabels)
            ? data.captureAlertLabels
            : Array.isArray(data?.CaptureAlertLabels)
                ? data.CaptureAlertLabels
                : Array.isArray(data?.Type)
                    ? data.Type
                : [];
        const fallbackAlertLabels = [
            data?.capture1AlertLabel ?? data?.Capture1AlertLabel ?? null,
            data?.capture2AlertLabel ?? data?.Capture2AlertLabel ?? null,
        ];
        const baseAlertLabels = rawCaptureAlertLabels.length > 0
            ? rawCaptureAlertLabels
            : fallbackAlertLabels;
        const captureAlertLabels = Array.from(
            { length: resolvedTotalCaptures },
            (_, index) => {
                const value = baseAlertLabels[index];
                return typeof value === 'string' && value.trim().length > 0 ? value : null;
            }
        );

        const workingCapturesInput = Number(data?.workingCaptures);
        const computedWorkingCaptures = captureStatuses.filter((status) => status === 'WORKING').length;
        const workingCaptures = Number.isFinite(workingCapturesInput)
            ? Math.max(0, Math.floor(workingCapturesInput))
            : computedWorkingCaptures;

        const maintenanceCaptureIndexInput = Number(data?.maintenanceCaptureIndex);
        const parsedMaintenanceCaptureIndex = Number.isFinite(maintenanceCaptureIndexInput)
            && maintenanceCaptureIndexInput > 0
            ? Math.floor(maintenanceCaptureIndexInput)
            : null;
        const maintenanceCaptureIndex = parsedMaintenanceCaptureIndex !== null
            && parsedMaintenanceCaptureIndex <= resolvedTotalCaptures
            ? parsedMaintenanceCaptureIndex
            : null;

        return {
            deviceId: data?.deviceId ?? (data?.DI !== undefined && data?.DI !== null ? String(data.DI) : ''),
            deviceName: data?.deviceName ?? data?.DN ?? null,
            deviceMatricule: data?.deviceMatricule ?? data?.DN ?? null,
            totalCaptures: resolvedTotalCaptures,
            workingCaptures,
            capture1Status: captureStatuses[0] ?? 'NOT_AVAILABLE',
            capture2Status: captureStatuses[1] ?? 'NOT_AVAILABLE',
            capture1LastErrorAt: captureLastErrorAt[0] ?? null,
            capture2LastErrorAt: captureLastErrorAt[1] ?? null,
            captureStatuses,
            captureLastErrorAt,
            captureAlertLabels,
            maintenanceCaptureIndex,
            isUnderMaintenance: Boolean(data?.isUnderMaintenance),
            maintenancePhase: typeof data?.maintenancePhase === 'string'
                ? data.maintenancePhase
                : null,
            maintenancePhaseStartedAt: data?.maintenancePhaseStartedAt ?? null,
            maintenanceStartedAt: data?.maintenanceStartedAt ?? null,
            maintenanceFinishedAt: data?.maintenanceFinishedAt ?? null,
            maintenanceEmployeeName: data?.maintenanceEmployeeName ?? null,
            lastUpdatedAt: data?.lastUpdatedAt ?? null,
        };
    }

    private isCaptureStatus(status: unknown): status is 'WORKING' | 'ERROR' | 'NOT_AVAILABLE' {
        return status === 'WORKING' || status === 'ERROR' || status === 'NOT_AVAILABLE';
    }
}
