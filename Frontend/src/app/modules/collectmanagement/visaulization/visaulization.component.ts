import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';
import {
    CaptureStatus,
    Device,
    DeviceCaptureState,
    DeviceStatusPayload,
} from '../../../core/device/device.model';
import { DeviceRealtimeService } from '../../../core/device/device-realtime.service';
import { DeviceService } from '../../../core/device/device.service';

interface SensorSlot {
    index: number;
    status: CaptureStatus;
    isUnderMaintenance: boolean;
    alertLabel?: string | null;
    alertFiredAt?: string | null;
    maintenanceEmployeeName?: string | null;
    maintenancePhase?: string | null;
    maintenancePhaseStartedAt?: string | null;
    maintenanceStartedAt?: string | null;
}

type DeviceFilter = 'all' | 'error' | 'maintenance' | 'ok';

@Component({
    selector: 'app-visaulization',
    standalone: true,
    imports: [MatIconModule],
    templateUrl: './visaulization.component.html',
    styleUrl: './visaulization.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisaulizationComponent implements OnInit, OnDestroy {
    devices: Device[] = [];
    captureStatesByDeviceId: Map<string, DeviceCaptureState> = new Map();
    isLoading = true;
    isRealtimeConnected = false;
    activeFilter: DeviceFilter = 'all';
    searchTerm = '';
    currentTime = Date.now();

    private _unsubscribeAll = new Subject<void>();

    constructor(
        private _deviceService: DeviceService,
        private _deviceRealtimeService: DeviceRealtimeService,
        private _changeDetectorRef: ChangeDetectorRef,
    ) { }

    ngOnInit(): void {
        this._deviceRealtimeService.connect();

        this._deviceRealtimeService.isConnected$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((isConnected) => {
                this.isRealtimeConnected = isConnected;
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService.captureStates$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((states) => {
                this.captureStatesByDeviceId = new Map(
                    (states ?? []).map((state) => [state.deviceId, state])
                );
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService
            .GetDeviceCaptureStateList()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe();

        this._deviceRealtimeService.deviceCaptureStateChanged$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((payload) => {
                this._deviceService.applyRealtimeCaptureState(payload);
            });

        this._deviceRealtimeService.deviceStatusChanged$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((payload) => {
                this.applyRealtimeDeviceStatus(payload);
            });

        this._deviceService
            .GetDevice(1, 100)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((paged) => {
                this.devices = paged?.devices ?? [];
                this.isLoading = false;
                this._changeDetectorRef.markForCheck();
            });

        // Update current time every second for real-time duration display
        import('rxjs').then(({ interval }) => {
            interval(1000)
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe(() => {
                    this.currentTime = Date.now();
                    this._changeDetectorRef.markForCheck();
                });
        });
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
    }

    get filteredDevices(): Device[] {
        return this.devices.filter((device) => this.matchesSearch(device) && this.matchesFilter(device));
    }

    setFilter(filter: DeviceFilter): void {
        this.activeFilter = filter;
    }

    onSearchInput(event: Event): void {
        const target = event.target as HTMLInputElement | null;
        this.searchTerm = (target?.value ?? '').trim().toLowerCase();
    }

    getDeviceState(device: Device): DeviceCaptureState | null {
        return this.captureStatesByDeviceId.get(device.deviceId) ?? null;
    }

    getDeviceStatusLabel(device: Device): string {
        const state = this.getDeviceState(device);
        if (!state) {
            return 'Etat inconnu';
        }

        if (state.isUnderMaintenance) {
            const phaseLabel = this.getMaintenancePhaseLabel(state.maintenancePhase);
            return phaseLabel ? `En maintenance - ${phaseLabel}` : 'En maintenance';
        }

        if (state.workingCaptures < state.totalCaptures) {
            const firstErrorIndex = state.captureStatuses.findIndex((status) => status === 'ERROR');
            const firstErrorLabel = firstErrorIndex >= 0
                ? state.captureAlertLabels?.[firstErrorIndex]
                : null;
            if (typeof firstErrorLabel === 'string' && firstErrorLabel.trim().length > 0) {
                return firstErrorLabel.trim();
            }
            return 'Alerte capture';
        }

        return 'En marche';
    }

    isDeviceInAlert(device: Device): boolean {
        const state = this.getDeviceState(device);
        return Boolean(state && state.workingCaptures < state.totalCaptures);
    }

    getDeviceStatusClass(device: Device): string {
        const state = this.getDeviceState(device);
        if (!state) {
            return 'device-card__status device-card__status--neutral';
        }

        if (state.isUnderMaintenance) {
            return 'device-card__status device-card__status--maintenance';
        }

        if (state.workingCaptures < state.totalCaptures) {
            return 'device-card__status device-card__status--error';
        }

        return 'device-card__status';
    }

    isDeviceUnderMaintenance(device: Device): boolean {
        const state = this.getDeviceState(device);
        return Boolean(state?.isUnderMaintenance);
    }

    sensorSlots(device: Device): SensorSlot[] {
        const state = this.getDeviceState(device);
        if (!state) {
            return this.buildFallbackSlots(device.nombreCapteur);
        }

        const total = Number.isFinite(Number(state.totalCaptures))
            ? Math.max(0, Math.floor(Number(state.totalCaptures)))
            : 0;

        return Array.from({ length: total }, (_, index) => ({
            index: index + 1,
            status: this.getCaptureStatusFromState(state, index + 1),
            isUnderMaintenance: this.isSensorUnderMaintenance(state, index + 1),
            alertLabel: this.getCaptureAlertLabelFromState(state, index + 1),
            alertFiredAt: this.getCaptureAlertFiredAtFromState(state, index + 1),
            maintenanceEmployeeName: state.maintenanceEmployeeName ?? null,
            maintenancePhase: state.maintenancePhase ?? null,
            maintenancePhaseStartedAt: state.maintenancePhaseStartedAt ?? null,
            maintenanceStartedAt: state.maintenanceStartedAt ?? null,
        }));
    }

    getCaptureClass(sensor: SensorSlot): string {
        if (sensor.status === 'ERROR') {
            return 'sensor-chip sensor-chip--error';
        }

        if (sensor.isUnderMaintenance && sensor.status === 'WORKING') {
            return 'sensor-chip sensor-chip--maintenance';
        }

        if (sensor.status === 'WORKING') {
            return 'sensor-chip sensor-chip--working';
        }

        return 'sensor-chip sensor-chip--not-available';
    }

    getCaptureClassTailwind(sensor: SensorSlot): string {
        if (sensor.status === 'ERROR') {
            return 'bg-red-50 border-red-200 shadow-[0_0_15px_-5px_rgba(239,68,68,0.15)]';
        }

        if (sensor.isUnderMaintenance && sensor.status === 'WORKING') {
            return 'bg-amber-50 border-amber-200';
        }

        if (sensor.status === 'WORKING') {
            return 'bg-emerald-50/50 border-emerald-100';
        }

        return 'bg-slate-50 border-slate-100 opacity-60';
    }

    isDeviceOffline(device: Device): boolean {
        return device.isOnline !== true;
    }

    getDeviceImagePath(device: Device): string {
        return this.isDeviceOffline(device)
            ? '/images/apps/device/device-disconnected.png'
            : '/images/apps/device/device-working.png';
    }

    getCaptureLabel(sensor: SensorSlot): string {
        if (sensor.status === 'ERROR') {
            return sensor.alertLabel?.trim() || 'Alerte détectée';
        }

        if (sensor.isUnderMaintenance && sensor.status === 'WORKING') {
            const phaseLabel = this.getMaintenancePhaseLabel(sensor.maintenancePhase);
            return phaseLabel ? `En Maintenance - ${phaseLabel}` : 'En Maintenance';
        }

        if (sensor.status === 'WORKING') {
            return 'Opérationnel';
        }

        return 'Inactif';
    }

    shouldShowMaintenanceIcon(sensor: SensorSlot): boolean {
        return sensor.isUnderMaintenance && sensor.status === 'WORKING';
    }

    getCaptureIndexLabel(sensor: SensorSlot): string {
        return `A${sensor.index}`;
    }

    getAlertDateTime(sensor: SensorSlot): string | null {
        if (!sensor.alertFiredAt) {
            return null;
        }
        return this.formatDateTime(sensor.alertFiredAt);
    }

    getAlertDuration(sensor: SensorSlot): string {
        if (!sensor.alertFiredAt) {
            return '-';
        }

        const start = new Date(sensor.alertFiredAt).getTime();
        if (!Number.isFinite(start)) {
            return '-';
        }

        const diffMs = Math.max(0, this.currentTime - start);
        return this.formatElapsedDuration(diffMs);
    }

    getMaintenanceDateTime(sensor: SensorSlot): string | null {
        const phaseStart = sensor.maintenancePhaseStartedAt ?? sensor.maintenanceStartedAt;
        if (!phaseStart) {
            return null;
        }
        return this.formatDateTime(phaseStart);
    }

    getMaintenanceDuration(sensor: SensorSlot): string {
        const phaseStart = sensor.maintenancePhaseStartedAt ?? sensor.maintenanceStartedAt;
        if (!phaseStart) {
            return '-';
        }

        const start = new Date(phaseStart).getTime();
        if (!Number.isFinite(start)) {
            return '-';
        }

        const diffMs = Math.max(0, this.currentTime - start);
        return this.formatElapsedDuration(diffMs);
    }

    private formatElapsedDuration(diffMs: number): string {
        const totalSeconds = Math.floor(diffMs / 1000);
        const seconds = totalSeconds % 60;
        const totalMinutes = Math.floor(totalSeconds / 60);
        const minutes = totalMinutes % 60;
        const totalHours = Math.floor(totalMinutes / 60);
        const hours = totalHours % 24;
        const days = Math.floor(totalHours / 24);

        const parts: string[] = [];

        if (days > 0) {
            parts.push(`${days}j`);
        }

        if (days > 0 || hours > 0) {
            parts.push(`${days > 0 ? String(hours).padStart(2, '0') : hours}h`);
        }

        if (days > 0 || hours > 0 || minutes > 0) {
            parts.push(`${String(minutes).padStart(2, '0')}m`);
        }

        parts.push(`${String(seconds).padStart(2, '0')}s`);

        return parts.join(' ');
    }

    getMaintenanceBackgroundColor(sensor: SensorSlot): string | null {
        if (!sensor.isUnderMaintenance) {
            return null;
        }

        const phase = this.normalizeMaintenancePhase(sensor.maintenancePhase);
        if (phase === 'AFFECTEE') {
            return '#d97706';
        }
        if (phase === 'DIAGNOSTIC') {
            return '#2563eb';
        }
        if (phase === 'REPARATION') {
            return '#059669';
        }

        return '#d97706';
    }

    getMaintenanceBorderColor(sensor: SensorSlot): string | null {
        return this.getMaintenanceBackgroundColor(sensor);
    }

    isMaintenancePhase(sensor: SensorSlot, phase: 'AFFECTEE' | 'DIAGNOSTIC' | 'REPARATION'): boolean {
        if (!sensor.isUnderMaintenance) {
            return false;
        }

        return this.normalizeMaintenancePhase(sensor.maintenancePhase) === phase;
    }

    hasKnownMaintenancePhase(sensor: SensorSlot): boolean {
        return this.normalizeMaintenancePhase(sensor.maintenancePhase) !== null;
    }

    getMaintenancePhaseLabel(phase: string | null | undefined): string {
        const normalized = this.normalizeMaintenancePhase(phase);
        if (normalized === 'AFFECTEE') {
            return 'Affectation';
        }
        if (normalized === 'DIAGNOSTIC') {
            return 'Diagnostic';
        }
        if (normalized === 'REPARATION') {
            return 'Reparation';
        }

        return '';
    }

    private normalizeMaintenancePhase(phase: string | null | undefined): 'AFFECTEE' | 'DIAGNOSTIC' | 'REPARATION' | null {
        if (!phase) {
            return null;
        }

        const normalized = phase
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .trim()
            .toUpperCase();

        if (normalized === 'AFFECTEE' || normalized === 'AFFECTE' || normalized === 'AFFECTATION') {
            return 'AFFECTEE';
        }
        if (normalized === 'DIAGNOSTIC' || normalized === 'DIAGNOSTIQUE') {
            return 'DIAGNOSTIC';
        }
        if (normalized === 'REPARATION') {
            return 'REPARATION';
        }

        return null;
    }

    formatDateTime(value: string): string {
        const parsed = new Date(value);
        const normalized = new Date(
            parsed.getFullYear(),
            parsed.getMonth(),
            parsed.getDate(),
            parsed.getHours(),
            parsed.getMinutes(),
            parsed.getSeconds()
        );
        return new Intl.DateTimeFormat('fr-FR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
        }).format(normalized);
    }

    private getCaptureStatusFromState(state: DeviceCaptureState, index: number): CaptureStatus {
        const statusFromArray = state.captureStatuses?.[index - 1];
        if (statusFromArray) {
            return statusFromArray;
        }

        if (index === 1) {
            return state.capture1Status ?? 'NOT_AVAILABLE';
        }

        if (index === 2) {
            return state.capture2Status ?? 'NOT_AVAILABLE';
        }

        return 'NOT_AVAILABLE';
    }

    private getCaptureAlertLabelFromState(state: DeviceCaptureState, index: number): string | null {
        const value = state.captureAlertLabels?.[index - 1];
        if (typeof value === 'string' && value.trim().length > 0) {
            return value;
        }

        return null;
    }

    private getCaptureAlertFiredAtFromState(state: DeviceCaptureState, index: number): string | null {
        const value = state.captureLastErrorAt?.[index - 1];
        if (typeof value === 'string' && value.trim().length > 0) {
            return value;
        }
        return null;
    }

    private isSensorUnderMaintenance(state: DeviceCaptureState, index: number): boolean {
        if (!state.isUnderMaintenance) {
            return false;
        }

        const totalCaptures = Math.max(0, Math.floor(Number(state.totalCaptures ?? 0)));
        if (
            state.maintenanceCaptureIndex
            && state.maintenanceCaptureIndex > 0
            && state.maintenanceCaptureIndex <= totalCaptures
        ) {
            return state.maintenanceCaptureIndex === index;
        }

        const firstWorkingCaptureIndex = state.captureStatuses.findIndex((status) => status === 'WORKING');
        const fallbackIndex = firstWorkingCaptureIndex >= 0 ? firstWorkingCaptureIndex + 1 : 1;
        return index === fallbackIndex;
    }

    private buildFallbackSlots(count: number | null | undefined): SensorSlot[] {
        const total = Number.isFinite(Number(count))
            ? Math.max(0, Math.floor(Number(count)))
            : 0;

        return Array.from({ length: total }, (_, index) => ({
            index: index + 1,
            status: 'NOT_AVAILABLE',
            isUnderMaintenance: false,
            alertLabel: null,
        }));
    }

    private applyRealtimeDeviceStatus(payload: DeviceStatusPayload): void {
        if (!payload?.deviceId) {
            return;
        }

        this.devices = this.devices.map((device) =>
            device.deviceId === payload.deviceId
                ? {
                    ...device,
                    isOnline: payload.isOnline,
                    lastSeen: payload.lastSeenAt,
                }
                : device
        );

        this._changeDetectorRef.markForCheck();
    }

    private matchesSearch(device: Device): boolean {
        if (!this.searchTerm) {
            return true;
        }

        const haystack = `${device.deviceName ?? ''} ${device.matricule ?? ''}`.toLowerCase();
        return haystack.includes(this.searchTerm);
    }

    private matchesFilter(device: Device): boolean {
        if (this.activeFilter === 'all') {
            return true;
        }

        if (this.activeFilter === 'maintenance') {
            return this.isDeviceUnderMaintenance(device);
        }

        if (this.activeFilter === 'error') {
            return this.isDeviceInAlert(device);
        }

        if (this.activeFilter === 'ok') {
            return !this.isDeviceUnderMaintenance(device) && !this.isDeviceInAlert(device);
        }

        return true;
    }
}

