import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { combineLatest, debounceTime, finalize, forkJoin, map, merge, Observable, of, startWith, Subject, switchMap, takeUntil } from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { FuseConfirmationService } from '../../../../@fuse/services/confirmation';
import { Alerte, GroupeWithEmployees } from '../../../core/alerte/alerte.model';
import { AlerteService } from '../../../core/alerte/alerte.service';
import { Type } from '../../../core/type/type.model';
import { TypeService } from '../../../core/type/type.service';
import { AsyncPipe, DatePipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { DeviceRealtimeService } from '../../../core/device/device-realtime.service';

@Component({
    selector: 'app-alerte',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressBarModule,
        MatSelectModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
        MatDialogModule,
    ],
    templateUrl: './alerte.component.html',
    styleUrl: './alerte.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class AlerteComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    alertes$: Observable<Alerte[]>;
    types: Type[] = [];
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    alertesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    statusFilterControl: UntypedFormControl = new UntypedFormControl('all');
    selectedAlerte: Alerte | null = null;
    selectedAlerteGroupes: GroupeWithEmployees[] = [];
    isLoadingEmployees: boolean = false;
    selectedAlerteForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    private _skipRealtimeRefreshUntil: number = 0;
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _alerteService: AlerteService,
        private _typeService: TypeService,
        private _activatedRoute: ActivatedRoute,
        private _deviceRealtimeService: DeviceRealtimeService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder,
        private _dialog: MatDialog
    ) { }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getAlertes()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getAlertes() {
        return this._alerteService.GetAlerte(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize ?? 50,
            this._sort?.active,
            this._sort?.direction,
            this.searchInputControl.value
        );
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    ngOnInit(): void {
        this.selectedAlerteForm = this._formBuilder.group({
            alerteId: [''],
            date: [null, [Validators.required]],
            device: [null],
            typeId: [null, [Validators.required]],
            dispositifName: [null],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this._typeService.GetType(1, 1000).subscribe((result) => {
            this.types = result?.types ?? [];
            this._changeDetectorRef.markForCheck();
        });

        this.alertes$ = combineLatest([
            this._alerteService.alertes$,
            this.statusFilterControl.valueChanges.pipe(startWith(this.statusFilterControl.value)),
        ]).pipe(
            map(([alertes, filter]) => {
                if (filter === 'non-traite') {
                    return (alertes ?? []).filter(a => !a.traiter);
                }
                if (filter === 'traite') {
                    return (alertes ?? []).filter(a => a.traiter);
                }
                return alertes;
            })
        );

        this._alerteService.alertesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.alertesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this._deviceRealtimeService.connect();
        merge(
            this._deviceRealtimeService.deviceCaptureStateChanged$,
            this._deviceRealtimeService.refreshMaintenance$
        )
            .pipe(debounceTime(150), takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                if (Date.now() < this._skipRealtimeRefreshUntil) {
                    return;
                }
                this.getAlertes().subscribe(() => {
                    this._changeDetectorRef.markForCheck();
                });
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getAlertes();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateAlerte() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._alerteService.CreateNewAlerte().subscribe((newAlerte) => {
            this.selectedAlerte = newAlerte;
            this.selectedAlerteForm.patchValue(newAlerte);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(alerteId: string): void {
        if (this.selectedAlerte && this.selectedAlerte.alerteId === alerteId) {
            this.closeDetails();
            return;
        }
        this._alerteService.GetAlerteById(alerteId).pipe(
            switchMap((alerte) => {
                this.selectedAlerte = alerte;
                this.selectedAlerteForm.patchValue(alerte);
                this.selectedAlerteGroupes = [];
                this.isLoadingEmployees = true;
                this._changeDetectorRef.markForCheck();

                if (alerte.date && alerte.dispositifId) {
                    return this._alerteService.GetEmployeesByPlanning(alerte.date, alerte.dispositifId);
                }
                return of([] as GroupeWithEmployees[]);
            })
        ).subscribe((groupes) => {
            this.selectedAlerteGroupes = groupes;
            this.isLoadingEmployees = false;
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedAlerte = null;
        this.selectedAlerteGroupes = [];
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedAlerte(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedAlerteForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const alerte = this.selectedAlerteForm.getRawValue();

        if (alerte.alerteId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._alerteService
                .AddAlerte(alerte)
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

        if (alerte.alerteId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._alerteService
                .UpdateAlerte(alerte)
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

    deleteSelectedAlerte(alerte: Alerte): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer cette alerte',
            message:
                'Êtes-vous sûr de vouloir supprimer cette alerte? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._alerteService
                    .DeleteAlerte({ alerteId: alerte.alerteId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    getTypeLabelById(typeId: string): string {
        return this.types.find(t => t.typeId === typeId)?.label ?? '';
    }

    trackByFn(index: number, item: any): any {
        return item.alerteId || index;
    }

    getActiveAlertsCount(alertes: Alerte[] | null | undefined): number {
        return (alertes ?? []).filter((item) => !item.traiter).length;
    }

    getTreatedAlertsCount(alertes: Alerte[] | null | undefined): number {
        return (alertes ?? []).filter((item) => item.traiter).length;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;

    openTraiterDialog(alerte: Alerte): void {
        import('./traiter-dialog/traiter-dialog.component').then(m => {
            const dialogRef = this._dialog.open(m.TraiterDialogComponent, {
                width: '760px',
                maxWidth: '95vw',
                data: {
                    alerteDate: alerte.date,
                    dispositifId: alerte.dispositifId,
                },
            });

            dialogRef.afterClosed().subscribe((result) => {
                if (!result) {
                    return;
                }
                this._skipRealtimeRefreshUntil = Date.now() + 5000;

                if (result.employeeId) {
                    this._alerteService
                        .TraiterAlerte(alerte.alerteId, result.employeeId)
                        .subscribe(() => {
                            this.closeDetails();
                            this._changeDetectorRef.markForCheck();
                        });
                } else if (result.employeeIds?.length) {
                    forkJoin(
                        result.employeeIds.map((id: string) =>
                            this._alerteService.TraiterAlerte(alerte.alerteId, id)
                        )
                    ).subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
                }
            });
        });
    }
}
