export interface MaintenanceStatItem {
    maintenanceId: string;
    deviceId?: string;
    employeeId?: string;
    deviceName?: string;
    employeeName?: string;
    t3Arrival?: string;
    t4Completion?: string;
    dureeReel: number;
    dureeTotalAlerte?: number;
    typeLabel?: string;
    dureeNominal?: number;
    ecart?: number;
    isDepassement: boolean;
}

export interface MaintenanceStatResponse {
    stats: MaintenanceStatItem[];
    length: number;
}

export interface KpiIndicatorsResponse {
    mttr: string;
    mttd: string;
    mttf: string;
    mtbf: string;
    nbAlert: number;
    nbPannes: number;
}
