import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { TranslocoModule } from '@ngneat/transloco';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { AsyncPipe, CommonModule, DatePipe, NgTemplateOutlet, NgClass } from '@angular/common';
import { ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { fuseAnimations } from '../../../../../@fuse/animations';
import { map, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import { Societe } from '../../../../core/societe/societe.model';
import { SocieteService } from '../../../../core/societe/societe.service';
import { FuseConfirmationService } from '../../../../../@fuse/services/confirmation';
import { SecurefilePipe } from '../../../../core/pipes/securefile.pipe';
import { RoleNavigation } from '../../../../core/role-utilisateur/role-utilisateur.model';
import { FuseNavigationAction } from '../../../../../@fuse/components/navigation';

@Component({
    selector: 'app-list',
    standalone: true,
    imports: [
        MatButtonModule,
        MatIconModule,
        ReactiveFormsModule,
        CommonModule,
        MatPaginatorModule,
        TranslocoModule,
        RouterLink,
        SecurefilePipe,
        MatFormFieldModule,
        MatInputModule,
        MatMenuModule,
        MatTooltipModule,
        MatSelect,
        DatePipe,
        NgClass,
    ],
    templateUrl: './list.component.html',
    styleUrl: './list.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class ListComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    societe$: Observable<Societe[]>;

    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    societeslength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roleNavigation: RoleNavigation;
    selectedSociete: Societe | null = null;
    isViewMode: boolean = false; // For details side panel

    // New State for View & Filters
    viewMode: 'list' | 'grid' = 'list';
    showFilters: boolean = false;
    sortBy: string = 'nom';
    sortOrder: 'asc' | 'desc' = 'asc';

    constructor(
        private _societeService: SocieteService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService
    ) { }

    SortChange() {
        this.isLoading = true;
        this.getSocietes().subscribe(() => {
            this.isLoading = false;
            this._changeDetectorRef.markForCheck();
        });
    }

    getSocietes() {
        return this._societeService.GetSociete(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize ?? 10,
            this.sortBy,
            this.sortOrder,
            this.searchInputControl.value
        );
    }

    toggleViewMode(): void {
        this.viewMode = this.viewMode === 'list' ? 'grid' : 'list';
    }

    toggleFilters(): void {
        this.showFilters = !this.showFilters;
    }

    applySort(sortBy: string, sortOrder: 'asc' | 'desc'): void {
        this.sortBy = sortBy;
        this.sortOrder = sortOrder;
        this.SortChange();
    }
    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }


    ngOnInit(): void {
        this.societe$ = this._societeService.societes$;

        this._societeService.societesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.societeslength = length;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });
        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.isLoading = true;
                    return this.getSocietes();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    /**
     * Toggle societe details for viewing (read-only mode)
     *
     * @param societeId
     */
    toggleDetails(societeId: string): void {
        //if the societe is already selected ...
        if (this.selectedSociete && this.selectedSociete.societeId === societeId) {
            // close the details
            this.closeDetails();
            return;
        }

        //Get the Societe by id
        this.societe$.pipe(
            map((Societes) => {
                const index = Societes.findIndex(item => item.societeId === societeId);
                return Societes[index];
            })
        )
            .subscribe((Societe) => {
                //set the selected societe
                this.selectedSociete = Societe;
                this.isViewMode = true; // Mode visualisation

                //Mark for check
                this._changeDetectorRef.markForCheck();
            });
    }

    /**
     * Edit societe - opens details in edit mode
     *
     * @param societeId
     */
    editSociete(societeId: string): void {
        //if the societe is already selected ...
        if (this.selectedSociete && this.selectedSociete.societeId === societeId) {
            // close the details
            this.closeDetails();
            return;
        }

        //Get the Societe by id
        this.societe$.pipe(
            map((Societes) => {
                const index = Societes.findIndex(item => item.societeId === societeId);
                return Societes[index];
            })
        )
            .subscribe((Societe) => {
                //set the selected societe
                this.selectedSociete = Societe;
                this.isViewMode = false; // Mode édition

                //Mark for check
                this._changeDetectorRef.markForCheck();
            });
    }

    /**
     * Close the details
     */
    closeDetails(): void {
        this.selectedSociete = null;
        this.isViewMode = false;
    }

    /**
     * Delete the selected product using the form data
     */
    deleteSelectedSociete(societe: Societe): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }
        // Open the confirmation dialog
        const confirmation = this._fuseConfirmationService.open({
            title: 'Delete Societe',
            message:
                'Are you sure you want to remove this position? This action cannot be undone!',
            actions: {
                confirm: {
                    label: 'Delete',
                },
            },
        });

        // Subscribe to the confirmation dialog closed action
        confirmation.afterClosed().subscribe((result) => {
            // If the confirm button pressed...
            if (result === 'confirmed') {
                // Delete the Fonction on the server
                this._societeService
                    .DeleteSociete({ societeId: societe.societeId })
                    .subscribe(() => {
                        // Mark for check
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
