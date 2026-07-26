import { SMSComponent } from './sms.component';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: SMSComponent,
        resolve: {
            navigation: (route: ActivatedRouteSnapshot) => inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'SMS'
    }
] as Routes;
