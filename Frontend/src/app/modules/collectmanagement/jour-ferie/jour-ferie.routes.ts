import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';
import { JourFerieComponent } from './jour-ferie.component';
import { jourFerieResolver } from './jour-ferie.resolver';

export default [
    {
        path: '',
        component: JourFerieComponent,
        resolve: {
            joursFeries: jourFerieResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Jour Férié',
    },
] as Routes;

