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
import { FuseConfirmationService } from '../../../../../@fuse/services/confirmation';
import { Planning } from '../../../../core/planning/planning.model';
import { PlanningService } from '../../../../core/planning/planning.service';
import { Groupe } from '../../../../core/groupe/groupe.model';
import { GroupeService } from '../../../../core/groupe/groupe.service';
import { Device } from '../../../../core/device/device.model';
import { DeviceService } from '../../../../core/device/device.service';
import { Shift } from '../../../../core/shift/shift.model';
import { ShiftService } from '../../../../core/shift/shift.service';
import { AsyncPipe, DatePipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { fuseAnimations } from '../../../../../@fuse/animations';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { FuseNavigationAction } from '../../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
    selector: 'app-planning-list',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatOptionModule,
        MatSelectModule,
        MatProgressBarModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
        MatTooltipModule,
        RouterLink,
        RouterLinkActive,
    ],
    templateUrl: './planning-list.component.html',
    styleUrl: './planning-list.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class PlanningListComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    plannings$: Observable<Planning[]>;
    groupes: Groupe[] = [];
    devices: Device[] = [];
    shifts: Shift[] = [];
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    planningsLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedPlanning: Planning | null = null;
    selectedPlanningForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _planningService: PlanningService,
        private _groupeService: GroupeService,
        private _deviceService: DeviceService,
        private _shiftService: ShiftService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    ngOnInit(): void {
        this.selectedPlanningForm = this._formBuilder.group({
            planningId: [''],
            date: ['', [Validators.required]],
            groupeIds: [[], [Validators.required]],
            deviceIds: [[]],
            shiftIds: [[], [Validators.required]],
        });

        // Get resolver data
        this._activatedRoute.data.pipe(takeUntil(this._unsubscribeAll)).subscribe((data) => {
            if (data?.navigation) {
                this.roleNavigation = data.navigation;
            }
            if (data?.plannings) {
                this.planningsLength = data.plannings.length || 0;
            }
            if (data?.groupes) {
                this.groupes = data.groupes.groupes || [];
            }
            if (data?.devices) {
                this.devices = data.devices.devices || [];
            }
            if (data?.shifts) {
                this.shifts = data.shifts.shifts || [];
            }
            this._changeDetectorRef.markForCheck();
        });

        this.plannings$ = this._planningService.plannings$;

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getPlannings();
                }),
                map(() => {
                    this.isLoading = false;
                })
            ).pipe(takeUntil(this._unsubscribeAll))
            .subscribe();
    }

    getPlannings() {
        return this._planningService.GetPlanning(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize || 10,
            this._sort?.active,
            this._sort?.direction,
            this.searchInputControl.value
        );
    }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getPlannings()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    getGroupeNames(groupeIds: string[] = []): string {
        return groupeIds
            .map((groupeId) => {
                const groupe = this.groupes.find((g) => g.groupeId === groupeId);
                return groupe ? groupe.nom : '';
            })
            .filter((name) => !!name)
            .join(', ');
    }

    getDeviceNames(deviceIds: string[] = []): string {
        return deviceIds
            .map((deviceId) => {
                const device = this.devices.find((d) => d.deviceId === deviceId);
                return device ? device.deviceName : '';
            })
            .filter((name) => !!name)
            .join(', ');
    }

    getShiftLabels(shiftIds: string[] = []): string {
        return shiftIds
            .map((shiftId) => {
                const shift = this.shifts.find((s) => s.shiftId === shiftId);
                return shift ? shift.label : '';
            })
            .filter((label) => !!label)
            .join(', ');
    }

    CreatePlanning() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._planningService.CreateNewPlanning().subscribe((newPlanning) => {
            this.selectedPlanning = newPlanning;
            this.selectedPlanningForm.reset(newPlanning);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(planningId: string): void {
        if (this.selectedPlanning && this.selectedPlanning.planningId === planningId) {
            this.closeDetails();
            return;
        }

        this._planningService.GetPlanningById(planningId).subscribe((planning) => {
            if (!planning) {
                this.closeDetails();
                return;
            }
            this.selectedPlanning = planning;
            this.selectedPlanningForm.reset(planning);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedPlanning = null;
        this.selectedPlanningForm.reset({
            planningId: '',
            date: '',
            groupeIds: [],
            deviceIds: [],
            shiftIds: [],
        });
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedPlanning(): void {
        if (!this.hasActionPermission(FuseNavigationAction.Edit) && !this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedPlanningForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const planning = this.selectedPlanningForm.getRawValue();

        if (planning.planningId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._planningService.AddPlanning(planning)
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

        if (planning.planningId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._planningService.UpdatePlanning(planning)
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

    deleteSelectedPlanning(planning: Planning): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            title: 'Supprimer cette planification',
            message: 'Êtes-vous sûr de vouloir supprimer cette planification? Cette action ne peut pas être annulée!',
            actions: {
                confirm: {
                    label: 'Supprimer',
                },
                cancel: {
                    label: 'Annuler',
                },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._planningService.DeletePlanning({ planningId: planning.planningId }).subscribe(() => {
                    this.closeDetails();
                    this._changeDetectorRef.markForCheck();
                });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.planningId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
