export interface Alerte {
    alerteId: string;
    date: string;
    dispositifId: string;
    typeId: string;
    dur?: number;
    Dur?: number;
    dispositifName?: string;
    traiter: boolean;
}

export interface PagedAlerte {
    alertes: Alerte[];
    length: number;
}

export interface EmployeePlanning {
    employeeId: string;
    nom: string;
    prenom: string;
    phone?: number;
    email?: string;
}

export interface GroupePlanning {
    groupeNom: string;
    shiftLabel: string;
    shiftStartTime: string;
    shiftEndTime: string;
    employees: EmployeePlanning[];
}

export interface GroupeWithEmployees {
    groupeNom: string;
    shiftLabel: string;
    shiftStartTime: string;
    shiftEndTime: string;
    employees: EmployeePlanning[];
}
