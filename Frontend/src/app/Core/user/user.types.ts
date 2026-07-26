import { RoleNavigation } from '../role-utilisateur/role-utilisateur.model';


export interface User {
    id: string;
    nomUtilisateur: string;
    nom: string;
    prenom: string;
    name: string;
    email: string;
    avatar?: string;
    status?: string;
    token:string;
    utilisateurId: string;
    societeId: string;
    navigations?: RoleNavigation[];
}
