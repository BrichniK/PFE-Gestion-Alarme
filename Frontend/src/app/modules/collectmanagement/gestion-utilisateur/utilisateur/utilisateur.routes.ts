import { UtilisateurComponent } from './utilisateur.component';
import { utilisateurResolver } from './utilisateur.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { RoleUtilisateurService } from '../../../../core/role-utilisateur/role-utilisateur.service';
import { UserService } from '../../../../core/user/user.service';

export default [
    {
        path: '',
        component:UtilisateurComponent,
        resolve:{
            utilisateurs:utilisateurResolver,
            roles:()=>inject(RoleUtilisateurService).GetAllRoleUtilisateur(),
            navigation:(route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Utilisateur'
    }
]as Routes;
