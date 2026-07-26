import { ShiftComponent } from './shift.component';
import { shiftResolver } from './shift.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: ShiftComponent,
        resolve: {
            shifts: shiftResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Shift',
    },
] as Routes;
