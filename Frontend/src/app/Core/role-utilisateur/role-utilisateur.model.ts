export interface RoleUtilisateur {
    roleUtilisateurId: string;
    libelleRoleUtilisateur: string;
    navigations?: RoleNavigation[];
}

export interface PagedRoleUtilisateur {
    rolesUtilisateur?: RoleUtilisateur[];
    length:number;
}

export interface RoleNavigation {
    navigationId:string;
    actions:number[];
    sections?: RoleSection[];
}

export interface RoleSection {
    sectionId:string;
    actions:number[];
}
