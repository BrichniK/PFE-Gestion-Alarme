export interface Shift {
    shiftId: string;
    label: string;
    startTime: string;
    endTime: string;
}

export interface PagedShift {
    shifts: Shift[];
    length: number;
}
