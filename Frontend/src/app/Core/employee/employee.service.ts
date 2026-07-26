import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedEmployee, Employee } from './employee.model';
import { ApiService } from '../common/api.service';

interface CreateEmployeeApiResponse {
    employeeId: string;
}

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    private _employees: BehaviorSubject<Employee[] | null> = new BehaviorSubject([]);
    private _employee: BehaviorSubject<Employee | null> = new BehaviorSubject(null);
    private _employeesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    get employees$(): Observable<Employee[]> {
        return this._employees.asObservable();
    }

    get employee$(): Observable<Employee> {
        return this._employee.asObservable();
    }

    get employeesLength$(): Observable<number> {
        return this._employeesLength.asObservable();
    }

    GetEmployee(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedEmployee> {
        return this._apiservice
            .Get<PagedEmployee>('employee/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._employees.next(result.data?.employees ?? []);
                    this._employeesLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewEmployee(): Observable<Employee> {
        const newEmployee: Employee = {
            employeeId: 'new',
            nom: null,
            prenom: null,
            phone: null,
            rfid: null,
            email: null,
            logoPath: null,
        };
        return of(newEmployee);
    }

    AddEmployee(employee: Employee): Observable<Employee> {
        const { employeeId, ...body } = employee;
        const normalizedBody = {
            ...body,
            phone: this.parsePhone(body.phone),
        };
        return this._apiservice.Post<CreateEmployeeApiResponse>('employee/add', normalizedBody).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newEmployee: Employee = {
                    ...normalizedBody,
                    employeeId: r.data?.employeeId,
                };
                this._employees.next([newEmployee, ...this._employees.value ?? []]);
                return newEmployee;
            })
        );
    }

    UpdateEmployee(employee: Employee): Observable<boolean> {
        const normalizedEmployee: Employee = {
            ...employee,
            phone: this.parsePhone(employee.phone),
        };
        return this._apiservice.Patch<boolean>('employee/update', normalizedEmployee).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._employees.value?.findIndex(
                    (item) => item.employeeId === normalizedEmployee.employeeId
                ) ?? -1;
                if (index !== -1) {
                    this._employees.value[index] = normalizedEmployee;
                    this._employees.next(this._employees.value);
                }
                return true;
            })
        );
    }

    DeleteEmployee(employee: { employeeId: string }): Observable<boolean> {
        if (employee.employeeId === 'new') {
            const index = this._employees.value.findIndex(
                (item) => item.employeeId === employee.employeeId
            );
            this._employees.value.splice(index, 1);
            this._employees.next(this._employees.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `employee/${employee.employeeId}/delete?employeeId=${employee.employeeId}`,
                employee
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._employees.value.findIndex(
                        (item) => item.employeeId === employee.employeeId
                    ) ?? -1;
                    if (index !== -1) {
                        this._employees.value.splice(index, 1);
                        this._employees.next(this._employees.value);
                    }
                    return true;
                })
            );
    }

    GetEmployeeById(id: string): Observable<Employee> {
        const localEmployee = this._employees.value?.find((x) => x.employeeId === id);
        if (localEmployee) {
            return of(localEmployee);
        }

        return this._apiservice.Get<Employee>(`employee/${id}/one`).pipe(
            map((r) => r.data),
            tap((employee) => {
                if (!employee) {
                    return;
                }
                const current = this._employees.value ?? [];
                const exists = current.some((item) => item.employeeId === employee.employeeId);
                if (!exists) {
                    this._employees.next([employee, ...current]);
                }
            })
        );
    }

    private parsePhone(phone: number | string | null | undefined): number {
        const parsedPhone = Number(phone);
        return Number.isFinite(parsedPhone) ? parsedPhone : 0;
    }
}
