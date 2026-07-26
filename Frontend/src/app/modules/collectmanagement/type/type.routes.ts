import { TypeComponent } from './type.component';
import { typeResolver } from './type.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: TypeComponent,
        resolve: {
            types: typeResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Type',
    },
] as Routes;
