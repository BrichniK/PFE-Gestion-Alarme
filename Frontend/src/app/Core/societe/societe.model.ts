export interface Societe{
    societeId:string;
    logoPath?:string;
    logoData?:string;
    logoExtension?:string;
    nom?:string;
    matriculeFiscal?:string;
    rne?:string;
    capital?:number;
    dateOverture:string;
    telephone1?:string;
    telephone2?:string;
    fax1?:string;
    fax2?:string;
    email?:string;
    adresse?:string;
    codeSociete?:string;
}

export interface PagedSociete{
    societes:Societe[];
    length:number;
}
