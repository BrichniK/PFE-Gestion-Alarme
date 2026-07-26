import { Injectable } from '@angular/core';
import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    IHttpConnectionOptions,
    LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { SettingConfigService } from '../config/setting-config.service';
import { DeviceCaptureStateRealtimePayload, DeviceStatusPayload } from './device.model';
import { MaintenanceCaptureRealtimePayload } from '../maintenance/maintenance.model';

@Injectable({
    providedIn: 'root',
})
export class DeviceRealtimeService {
    private _hubConnection: HubConnection | null = null;
    private _starting = false;

    private _isConnected: BehaviorSubject<boolean> = new BehaviorSubject(false);
    private _deviceCaptureStateChanged: Subject<DeviceCaptureStateRealtimePayload> = new Subject();
    private _deviceStatusChanged: Subject<DeviceStatusPayload> = new Subject();
    private _maintenanceCaptureUpdated: Subject<MaintenanceCaptureRealtimePayload> = new Subject();
    private _refreshMaintenance: Subject<any> = new Subject();

    constructor(
        private _settingConfigService: SettingConfigService,
        private _authService: AuthService,
    ) {}

    get isConnected$(): Observable<boolean> {
        return this._isConnected.asObservable();
    }

    get deviceCaptureStateChanged$(): Observable<DeviceCaptureStateRealtimePayload> {
        return this._deviceCaptureStateChanged.asObservable();
    }

    get deviceStatusChanged$(): Observable<DeviceStatusPayload> {
        return this._deviceStatusChanged.asObservable();
    }

    get maintenanceCaptureUpdated$(): Observable<MaintenanceCaptureRealtimePayload> {
        return this._maintenanceCaptureUpdated.asObservable();
    }

    get refreshMaintenance$(): Observable<any> {
        return this._refreshMaintenance.asObservable();
    }

    connect(): void {
        if (this._starting) {
            return;
        }

        if (
            this._hubConnection &&
            (this._hubConnection.state === HubConnectionState.Connected ||
                this._hubConnection.state === HubConnectionState.Connecting ||
                this._hubConnection.state === HubConnectionState.Reconnecting)
        ) {
            return;
        }

        this._starting = true;
        this._hubConnection = this.buildConnection();
        this.registerHandlers(this._hubConnection);

        this._hubConnection
            .start()
            .then(() => this._isConnected.next(true))
            .catch((error) => {
                console.error('SignalR connection start failed:', error);
                this._isConnected.next(false);
            })
            .finally(() => {
                this._starting = false;
            });
    }

    disconnect(): void {
        if (!this._hubConnection) {
            return;
        }

        this._hubConnection
            .stop()
            .catch((error) => console.error('SignalR connection stop failed:', error))
            .finally(() => {
                this._hubConnection = null;
                this._isConnected.next(false);
            });
    }

    private buildConnection(): HubConnection {
        const options: IHttpConnectionOptions = {
            accessTokenFactory: () => this._authService.accessToken ?? '',
        };

        return new HubConnectionBuilder()
            .withUrl(this.resolveHubUrl(), options)
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(LogLevel.Warning)
            .build();
    }

    private registerHandlers(connection: HubConnection): void {
        connection.on('DeviceCaptureStateChanged', (payload: DeviceCaptureStateRealtimePayload) => {
            this._deviceCaptureStateChanged.next(payload);
        });

        connection.on('DeviceStatusChanged', (payload: DeviceStatusPayload) => {
            this._deviceStatusChanged.next(payload);
        });

        connection.on('MaintenanceCaptureUpdated', (payload: MaintenanceCaptureRealtimePayload) => {
            this._maintenanceCaptureUpdated.next(payload);
        });

        connection.on('RefreshMaintenance', (payload: any) => {
            this._refreshMaintenance.next(payload);
        });

        connection.onreconnecting(() => {
            this._isConnected.next(false);
        });

        connection.onreconnected(() => {
            this._isConnected.next(true);
        });

        connection.onclose(() => {
            this._isConnected.next(false);
        });
    }

    private resolveHubUrl(): string {
        const baseApi = this._settingConfigService.baseApi || environment.BaseApi;
        return `${baseApi.replace(/\/$/, '')}/signalHub`;
    }
}
