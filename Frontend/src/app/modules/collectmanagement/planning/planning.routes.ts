import { PlanningComponent } from './planning.component';
import { PlanningCalendarComponent } from './planning-calendar/planning-calendar.component';
import { planningResolver } from './planning.resolver';
import { ActivatedRouteSnapshot, Routes } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../../../core/user/user.service';
import { GroupeService } from '../../../core/groupe/groupe.service';
import { DeviceService } from '../../../core/device/device.service';
import { ShiftService } from '../../../core/shift/shift.service';
import { JourFerieService } from '../../../core/jour-ferie/jour-ferie.service';
import { EmployeeService } from '../../../core/employee/employee.service';

export default [
    {
        path: '',
        component: PlanningComponent,
        resolve: {
            plannings: planningResolver,
            groupes: () => inject(GroupeService).GetGroupe(1, 10000),
            devices: () => inject(DeviceService).GetDevice(1, 10000),
            shifts: () => inject(ShiftService).GetShift(1, 10000),
            holidays: () => inject(JourFerieService).GetJourFerie(1, 10000),
            employees: () => inject(EmployeeService).GetEmployee(1, 10000),
            navigation: (route: ActivatedRouteSnapshot) =>
                inject(UserService).getNavigation(route.data.navigationId),
        },
        title: 'Planning',
        children: [
            {
                path: '',
                component: PlanningCalendarComponent,
                resolve: {
                    plannings: planningResolver,
                    groupes: () => inject(GroupeService).GetGroupe(1, 10000),
                    devices: () => inject(DeviceService).GetDevice(1, 10000),
                    shifts: () => inject(ShiftService).GetShift(1, 10000),
                    holidays: () => inject(JourFerieService).GetJourFerie(1, 10000),
                    employees: () => inject(EmployeeService).GetEmployee(1, 10000),
                    navigation: (route: ActivatedRouteSnapshot) =>
                        inject(UserService).getNavigation(route.data.navigationId),
                },
                title: 'Planning Calendrier',
            },
        ],
    },
] as Routes;
