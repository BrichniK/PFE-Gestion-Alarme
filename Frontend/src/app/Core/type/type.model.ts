export interface Type {
    typeId: string;
    code: string;
    label: string;
    dureeNominal?: number;
}

export interface PagedType {
    types: Type[];
    length: number;
}
