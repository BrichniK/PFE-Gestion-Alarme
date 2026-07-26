import { MaintenanceComponent } from './maintenance.component';
import { maintenanceResolver } from './maintenance.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';
import { EmployeeService } from '../../../core/employee/employee.service';
import { DeviceService } from '../../../core/device/device.service';

export default [
    {
        path: '',
        component: MaintenanceComponent,
        resolve: {
            maintenances: maintenanceResolver,
            employees: () => inject(EmployeeService).GetEmployee(),
            devices: () => inject(DeviceService).GetDevice(),
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Maintenance',
    },
] as Routes;
