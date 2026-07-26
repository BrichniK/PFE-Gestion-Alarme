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
import { Groupe } from '../../../core/groupe/groupe.model';
import { GroupeService } from '../../../core/groupe/groupe.service';
import { Employee } from '../../../core/employee/employee.model';
import { EmployeeService } from '../../../core/employee/employee.service';
import { AsyncPipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
    selector: 'app-groupe',
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
        MatCheckboxModule,
        TranslocoDirective,
        MatTooltip,
    ],
    templateUrl: './groupe.component.html',
    styleUrl: './groupe.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class GroupeComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    groupes$: Observable<Groupe[]>;
    employees: Employee[] = [];
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    groupesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedGroupe: Groupe | null = null;
    selectedGroupeForm: UntypedFormGroup;
    selectedEmployeeIds: string[] = [];
    employeeSearchControl: UntypedFormControl = new UntypedFormControl();
    filteredEmployees: Employee[] = [];
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _groupeService: GroupeService,
        private _employeeService: EmployeeService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    ngOnInit(): void {
        this.selectedGroupeForm = this._formBuilder.group({
            groupeId: [''],
            nom: [null, [Validators.required]],
            color: ['#2E6C9F'],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this._employeeService.employees$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((employees) => {
                this.employees = employees ?? [];
                this.filteredEmployees = [...this.employees];
                this._changeDetectorRef.markForCheck();
            });

        this.employeeSearchControl.valueChanges
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((search) => {
                if (!search) {
                    this.filteredEmployees = [...this.employees];
                } else {
                    const lower = search.toLowerCase();
                    this.filteredEmployees = this.employees.filter(
                        (e) =>
                            e.nom?.toLowerCase().includes(lower) ||
                            e.prenom?.toLowerCase().includes(lower)
                    );
                }
                this._changeDetectorRef.markForCheck();
            });

        this.groupes$ = this._groupeService.groupes$;

        this._groupeService.groupesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.groupesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getGroupes();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getGroupes()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getGroupes() {
        return this._groupeService.GetGroupe(
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

    CreateGroupe() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._groupeService.CreateNewGroupe().subscribe((newGroupe) => {
            this.selectedGroupe = newGroupe;
            this.selectedGroupeForm.reset({ groupeId: newGroupe.groupeId, nom: null, color: '#2E6C9F' });
            this.selectedEmployeeIds = [];
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(groupeId: string): void {
        if (this.selectedGroupe && this.selectedGroupe.groupeId === groupeId) {
            this.closeDetails();
            return;
        }
        this._groupeService.GetGroupeById(groupeId).subscribe((groupe) => {
            this.selectedGroupe = groupe;
            this.selectedGroupeForm.reset({ groupeId: groupe.groupeId, nom: groupe.nom, color: groupe.color || '#2E6C9F' });
            this.selectedEmployeeIds = [...(groupe.employeeIds || [])];
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedGroupe = null;
        this.selectedGroupeForm.reset({ groupeId: '', nom: null, color: '#2E6C9F' });
        this.selectedEmployeeIds = [];
        this.employeeSearchControl.reset();
        this._changeDetectorRef.markForCheck();
    }

    isEmployeeSelected(employeeId: string): boolean {
        return this.selectedEmployeeIds.includes(employeeId);
    }

    toggleEmployee(employeeId: string): void {
        const index = this.selectedEmployeeIds.indexOf(employeeId);
        if (index === -1) {
            this.selectedEmployeeIds.push(employeeId);
        } else {
            this.selectedEmployeeIds.splice(index, 1);
        }
        this._changeDetectorRef.markForCheck();
    }

    getEmployeeName(employeeId: string): string {
        const emp = this.employees.find((e) => e.employeeId === employeeId);
        return emp ? `${emp.nom} ${emp.prenom}` : employeeId;
    }

    SaveSelectedGroupe(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedGroupeForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const formValue = this.selectedGroupeForm.getRawValue();
        const groupe: Groupe = {
            ...formValue,
            employeeIds: this.selectedEmployeeIds,
        };

        if (groupe.groupeId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._groupeService
                .AddGroupe(groupe)
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

        if (groupe.groupeId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._groupeService
                .UpdateGroupe(groupe)
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

    deleteSelectedGroupe(groupe: Groupe): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer ce groupe',
            message:
                'Êtes-vous sûr de vouloir supprimer ce groupe? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._groupeService
                    .DeleteGroupe({ groupeId: groupe.groupeId })
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
        return item.groupeId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
