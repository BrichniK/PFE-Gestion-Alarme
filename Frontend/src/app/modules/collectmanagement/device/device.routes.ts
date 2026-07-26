import { DeviceComponent } from './device.component';
import { deviceResolver } from './device.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: DeviceComponent,
        resolve: {
            devices: deviceResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Device',
    },
] as Routes;
