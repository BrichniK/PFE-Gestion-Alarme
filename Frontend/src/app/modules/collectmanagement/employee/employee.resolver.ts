import { ResolveFn } from '@angular/router';
import { PagedEmployee } from '../../../core/employee/employee.model';
import { inject } from '@angular/core';
import { EmployeeService } from '../../../core/employee/employee.service';

export const employeeResolver: ResolveFn<PagedEmployee> = (route, state) => {
    return inject(EmployeeService).GetEmployee();
};
