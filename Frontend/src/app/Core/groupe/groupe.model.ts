export interface Groupe {
    groupeId: string;
    nom: string;
    color: string;
    employeeIds: string[];
}

export interface PagedGroupe {
    groupes: Groupe[];
    length: number;
}
