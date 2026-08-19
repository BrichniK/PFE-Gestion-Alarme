export interface Device {
    deviceId: string;
    deviceName: string;
    matricule: string;
    nombreCapteur: number;
    isOnline?: boolean;
    lastSeen?: string;
}

export interface PagedDevice {
    devices: Device[];
    length: number;
}

export type CaptureStatus =
    | 'WORKING'
    | 'ERROR'
    | 'NOT_AVAILABLE';

export type DeviceCaptureStateTrigger =
    | 'ALARM_CAPTURE'
    | 'MAINTENANCE_T3'
    | 'MAINTENANCE_T4';

export interface DeviceCaptureState {
    deviceId: string;
    deviceName?: string;
    deviceMatricule?: string;

    totalCaptures: number;
    workingCaptures: number;

    capture1Status?: CaptureStatus;
    capture2Status?: CaptureStatus;

    capture1LastErrorAt?: string | null;
    capture2LastErrorAt?: string | null;

    captureStatuses?: CaptureStatus[];
    captureLastErrorAt?: (string | null)[];
    captureAlertLabels?: (string | null)[];

    maintenanceCaptureIndex?: number | null;

    isUnderMaintenance: boolean;

    maintenancePhase?:
        | 'AFFECTEE'
        | 'DIAGNOSTIC'
        | 'REPARATION'
        | null;

    maintenancePhaseStartedAt?: string | null;
    maintenanceStartedAt?: string | null;
    maintenanceFinishedAt?: string | null;

    maintenanceEmployeeName?: string | null;

    lastUpdatedAt?: string | null;
}

export interface DeviceCaptureStateRealtimePayload
    extends DeviceCaptureState {
    trigger?: DeviceCaptureStateTrigger | string;
}

export interface DeviceStatusPayload {
    deviceId: string;
    deviceName?: string;
    deviceMatricule?: string;
    isOnline: boolean;
    lastSeenAt: string;
}