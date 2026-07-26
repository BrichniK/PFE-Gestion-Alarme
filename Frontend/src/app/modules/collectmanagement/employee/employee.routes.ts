import { EmployeeComponent } from './employee.component';
import { employeeResolver } from './employee.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';

export default [
    {
        path: '',
        component: EmployeeComponent,
        resolve: {
            employees: employeeResolver,
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Employee',
    },
] as Routes;
