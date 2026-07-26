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
import { JourFerie } from '../../../core/jour-ferie/jour-ferie.model';
import { JourFerieService } from '../../../core/jour-ferie/jour-ferie.service';
import { AsyncPipe, DatePipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
    selector: 'app-jour-ferie',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        MatButtonModule,
        MatIconModule,
        MatProgressBarModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
        MatTooltipModule,
    ],
    templateUrl: './jour-ferie.component.html',
    styleUrl: './jour-ferie.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class JourFerieComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    joursFeries$: Observable<JourFerie[]>;
    isLoading: boolean = false;
    joursFeriesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedJourFerie: JourFerie | null = null;
    selectedJourFerieForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _jourFerieService: JourFerieService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getJoursFeries()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getJoursFeries() {
        return this._jourFerieService.GetJourFerie(
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
        this.selectedJourFerieForm = this._formBuilder.group({
            jourFerieId: [''],
            date: [null, [Validators.required]],
            label: [null, [Validators.required]],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this.joursFeries$ = this._jourFerieService.joursFeries$;

        this._jourFerieService.joursFeriesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.joursFeriesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.isLoading = true;
        this._jourFerieService
            .GetJourFerie(1, 10)
            .pipe(
                finalize(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                }),
                takeUntil(this._unsubscribeAll)
            )
            .subscribe();

        this.searchInputControl.valueChanges
            .pipe(
                switchMap(() => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getJoursFeries();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateJourFerie() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._jourFerieService.CreateNewJourFerie().subscribe((newJourFerie) => {
            this.selectedJourFerie = newJourFerie;
            this.selectedJourFerieForm.reset(newJourFerie);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(jourFerieId: string): void {
        if (this.selectedJourFerie && this.selectedJourFerie.jourFerieId === jourFerieId) {
            this.closeDetails();
            return;
        }
        this._jourFerieService.GetJourFerieById(jourFerieId).subscribe((jourFerie) => {
            this.selectedJourFerie = jourFerie;
            this.selectedJourFerieForm.reset(jourFerie);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedJourFerie = null;
        this.selectedJourFerieForm.reset({
            jourFerieId: '',
            date: null,
            label: null,
        });
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedJourFerie(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedJourFerieForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const jourFerie = this.selectedJourFerieForm.getRawValue();

        if (jourFerie.jourFerieId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._jourFerieService
                .AddJourFerie(jourFerie)
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

        if (jourFerie.jourFerieId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._jourFerieService
                .UpdateJourFerie(jourFerie)
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

    deleteSelectedJourFerie(jourFerie: JourFerie): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer ce jour férié',
            message:
                'Êtes-vous sûr de vouloir supprimer ce jour férié? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._jourFerieService
                    .DeleteJourFerie({ jourFerieId: jourFerie.jourFerieId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.jourFerieId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
