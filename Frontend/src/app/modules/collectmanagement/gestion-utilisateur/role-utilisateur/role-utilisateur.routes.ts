import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { catchError, of } from 'rxjs';
import { RoleUtilisateurComponent } from './role-utilisateur.component';
import { RoleUtilisateurListComponent } from './list/list.component';
import { RoleUtilisateurDetailsComponent } from './details/details.component';
import { RoleUtilisateurService } from '../../../../core/role-utilisateur/role-utilisateur.service';
import { NavigationService } from '../../../../core/navigation/navigation.service';
import { UserService } from '../../../../core/user/user.service';

const roleResolver = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
) => {
    const roleUtilisateurService = inject(RoleUtilisateurService);
    const router = inject(Router);

    const roleId = route.paramMap.get('rid');

    if (roleId === 'ajouter'){
        return roleUtilisateurService.CreateNewRoleUtilisateur();
    }

    return roleUtilisateurService.GetOneRoleUtilisateur(roleId)
        .pipe(

            catchError(async (error) => {

                const parentUrl = state.url.split('/').slice(0, -1).join('/');

                await router.navigateByUrl(parentUrl);

                return of(error);
            })
        );
};

export default [
    {
        path     : '',
        component: RoleUtilisateurComponent,
        children:[
            {
                path: ':rid',
                data: { navigationId: 'fichier.role-utilisateur' },
                component: RoleUtilisateurDetailsComponent,
                resolve: {
                    role: roleResolver,
                    navigation:(route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),
                    actions: () => inject(RoleUtilisateurService).GetAction(),
                },
                title: 'Rôle Utilisateur',
            },
            {
                path     : '',
                data: { navigationId: 'fichier.role-utilisateur' },
                component: RoleUtilisateurListComponent,
                resolve: {
                    roles: () => inject(RoleUtilisateurService).GetRoleUtilisateur(),
                    navigation:(route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),
                },
                title: 'Rôles Utilisateur',
            },

        ]
    }
]as Routes;
