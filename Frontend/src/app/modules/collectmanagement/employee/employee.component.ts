import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { finalize, map, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { FuseConfirmationService } from '../../../../@fuse/services/confirmation';
import { Employee } from '../../../core/employee/employee.model';
import { EmployeeService } from '../../../core/employee/employee.service';
import { AsyncPipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'app-employee',
    standalone: true,
    imports: [
        AsyncPipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressBarModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
    ],
    templateUrl: './employee.component.html',
    styleUrl: './employee.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class EmployeeComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    employees$: Observable<Employee[]>;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    employeesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedEmployee: Employee | null = null;
    selectedEmployeeForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _employeeService: EmployeeService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getEmployees()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getEmployees() {
        return this._employeeService.GetEmployee(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize,
            this._sort?.active,
            this._sort?.direction,
            this.searchInputControl.value
        );
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    ngOnInit(): void {
        this.selectedEmployeeForm = this._formBuilder.group({
            employeeId: [''],
            nom: [null, [Validators.required]],
            prenom: [null, [Validators.required]],
            phone: [null, [Validators.required]],
            rfid: [null],
            email: [null],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this.employees$ = this._employeeService.employees$;

        this._employeeService.employeesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.employeesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getEmployees();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateEmployee() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._employeeService.CreateNewEmployee().subscribe((newEmployee) => {
            this.selectedEmployee = newEmployee;
            this.selectedEmployeeForm.reset(newEmployee);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(employeeId: string): void {
        if (this.selectedEmployee && this.selectedEmployee.employeeId === employeeId) {
            this.closeDetails();
            return;
        }
        this._employeeService.GetEmployeeById(employeeId).subscribe((employee) => {
            this.selectedEmployee = employee;
            this.selectedEmployeeForm.reset(employee);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedEmployee = null;
        this.selectedEmployeeForm.reset({
            employeeId: '',
            nom: null,
            prenom: null,
            phone: null,
            rfid: null,
            email: null,
        });
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedEmployee(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedEmployeeForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const employee = this.selectedEmployeeForm.getRawValue();

        if (employee.employeeId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._employeeService
                .AddEmployee(employee)
                .pipe(
                    finalize(() => {
                        this.saveClicked = false;
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }

        if (employee.employeeId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._employeeService
                .UpdateEmployee(employee)
                .pipe(
                    finalize(() => {
                        this.saveClicked = false;
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }
    }

    deleteSelectedEmployee(employee: Employee): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer cet employé',
            message:
                'Êtes-vous sûr de vouloir supprimer cet employé? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._employeeService
                    .DeleteEmployee({ employeeId: employee.employeeId })
                    .subscribe((isDeleted) => {
                        if (isDeleted) {
                            this.closeDetails();
                        }
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.employeeId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
