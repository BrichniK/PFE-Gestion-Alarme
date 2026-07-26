export interface SMS {
    smsId: string;
    nomPrenom: string;
    phoneNumber: string;
    devices: Device[];
}

export interface Device {
    deviceId: string;
    deviceName: string;
    matricule: string;
}

export interface PagedSMS {
    // Backend returns `SMSs` which can serialize to: smsS / smSs / smss depending on naming policy
    smsS?: SMS[];
    smSs?: SMS[];
    smss?: SMS[];
    length: number;
}
