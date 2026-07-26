import { GroupeComponent } from './groupe.component';
import { groupeResolver } from './groupe.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';
import { EmployeeService } from '../../../core/employee/employee.service';

export default [
    {
        path: '',
        component: GroupeComponent,
        resolve: {
            groupes: groupeResolver,
            employees: () => inject(EmployeeService).GetEmployee(1, 1000),
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Groupes',
    },
] as Routes;
