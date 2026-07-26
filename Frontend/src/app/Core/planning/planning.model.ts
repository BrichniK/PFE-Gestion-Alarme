export interface Planning {
    planningId: string;
    date: string | null;
    dates?: string[];
    assignmentMode?: 'group' | 'employee';
    groupeIds: string[];
    employeeIds: string[];
    groupeColors?: string[];
    deviceIds: string[];
    shiftIds: string[];
    groupes?: PlanningGroupe[];
    devices?: PlanningDevice[];
    shifts?: PlanningShift[];
}

export interface PagedPlanning {
    plannings: Planning[];
    length: number;
}

export interface PlanningGroupe {
    groupeId: string;
    groupeNom?: string;
    groupeColor?: string;
}

export interface PlanningDevice {
    deviceId: string;
    deviceName?: string;
}

export interface PlanningShift {
    shiftId: string;
    shiftLabel?: string;
}
