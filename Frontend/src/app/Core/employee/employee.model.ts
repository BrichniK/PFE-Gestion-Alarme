export interface Employee {
    employeeId: string;
    nom: string;
    prenom: string;
    phone: number;
    rfid: string;
    email?: string;
    logoPath?: string;
    logoData?: string;
    logoExtension?: string;
}

export interface PagedEmployee {
    employees: Employee[];
    length: number;
}
