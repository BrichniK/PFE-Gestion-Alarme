export interface Maintenance {
    maintenanceId: string;
    deviceId: string;
    employeeId: string;
    t1Alerte?: string;
    t2Assignment?: string;
    t3Arrival?: string;
    t4Completion?: string;
    t5Confirmation?: string;
    t6NextAlert?: string;
    description: string;
    deviceName?: string;
    employeeNom?: string;
    employeePrenom?: string;
}

export interface PagedMaintenance {
    maintenances: Maintenance[];
    length: number;
}

export interface MaintenanceRfidResponse {
    success: boolean;
    message: string;
    stepCompleted?: string;
    nextStep?: string;
    employeeNom?: string;
    employeePrenom?: string;
    employeeRfid?: string;
    maintenanceId?: string;
    t1Alerte?: string;
    t2Assignment?: string;
    t3Arrival?: string;
    t4Completion?: string;
    t5Confirmation?: string;
}

export type MaintenanceCaptureStatus = 'IN_MAINTENANCE' | 'IN_CONFIRMATION' | 'FINISHED';

export interface DeviceCaptureHistoryItem {
    maintenanceId?: string;
    deviceId: string;
    deviceName?: string;
    deviceMatricule?: string;
    employeeId?: string;
    employeeNom?: string;
    employeePrenom?: string;
    tagRfid?: string;
    capture1Status?: string;
    capture2Status?: string;
    status?: MaintenanceCaptureStatus | string;
    maintenanceStartedAt?: string | null;
    maintenanceFinishedAt?: string | null;
    lastUpdatedAt?: string | null;
}

export interface MaintenanceCaptureRealtimePayload extends DeviceCaptureHistoryItem {}
