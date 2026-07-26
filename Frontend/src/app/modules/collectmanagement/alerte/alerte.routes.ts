import { AlerteComponent } from './alerte.component';
import { alerteResolver } from './alerte.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: AlerteComponent,
        resolve: {
            alertes: alerteResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Alerte',
    },
] as Routes;
