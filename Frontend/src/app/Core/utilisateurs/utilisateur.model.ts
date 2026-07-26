export interface Utilisateur {
    utilisateurId: string;
    nomUtilisateur: string;
    nom: string;
    prenom: string;
    email: string;
    password?: string;
    roleUtilisateurId?: string;
    libelleRoleUtilisateur?:string;
    isActive:boolean;
    etat:number;
    societeId: string;


}

export interface PagedUtilisateur {
    utilisateurs:Utilisateur[];
    length:number;

}
