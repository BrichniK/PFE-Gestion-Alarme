import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { catchError, of } from 'rxjs';
import { SocieteComponent } from './societe.component';
import { ListComponent } from './list/list.component';
import { DetailsComponent } from './details/details.component';
import { SocieteService } from '../../../core/societe/societe.service';
import { UserService } from '../../../core/user/user.service';


const societeResolver = (
    route:ActivatedRouteSnapshot,
    state:RouterStateSnapshot
)=>{
    const societeService = inject(SocieteService);
    const router = inject(Router);

    const id  = route.paramMap.get('id')

    if(id === 'ajouter'){
        return societeService.CreateNewSociete();
    }

    return societeService.GetSocieteById(id)
        .pipe(

            catchError(async (error) =>{

                const parentUrl = state.url.split('/').slice(0, -1).join('/');

                await router.navigateByUrl(parentUrl);

                return of(error);
            })
        );
};

/**
 * Can deactivate SocieteEtablissement
 *
 * @param component
 * @param currentRoute
 * @param currentState
 * @param nextState
 */

const canDeactivateClientSite = (
    currentRoute: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot,
    nextState: RouterStateSnapshot
) => {
    // Get the next route
    let nextRoute: ActivatedRouteSnapshot = nextState.root;

    while (nextRoute.firstChild) {
        nextRoute = nextRoute.firstChild;
    }

    // If we are navigating to another client...
    if (!nextState.url.endsWith('/societe/'+nextRoute.paramMap.get('id')) ) {
        // Just navigate
        return true;
    }

};

export default [
    {
        path     : '',
        component: SocieteComponent,
        children:[
            {
                path     : '',
                component: ListComponent,
                resolve: {
                    societes: () => inject(SocieteService).GetSociete(),
                    navigation:(route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),
                },
                title: 'Societes',
            },
            {
                path     : ':id',
                component: DetailsComponent,
                resolve: {
                    societe: societeResolver,
                    navigation:(route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),

                },
                title: 'Societe',
            }
        ]
    }
]as Routes;
