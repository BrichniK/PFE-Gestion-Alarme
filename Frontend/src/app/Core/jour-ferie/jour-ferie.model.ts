export interface JourFerie {
    jourFerieId: string;
    date: string;
    label: string;
}

export interface PagedJourFerie {
    joursFeries: JourFerie[];
    length: number;
}
