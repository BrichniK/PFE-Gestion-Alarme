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
import { Type } from '../../../core/type/type.model';
import { TypeService } from '../../../core/type/type.service';
import { AsyncPipe, NgClass, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'app-type',
    standalone: true,
    imports: [
        AsyncPipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressBarModule,
        MatSortModule,
        MatTooltipModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
    ],
    templateUrl: './type.component.html',
    styleUrl: './type.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class TypeComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    types$: Observable<Type[]>;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    typesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedType: Type | null = null;
    selectedTypeForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _typeService: TypeService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) { }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getTypes()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getTypes() {
        return this._typeService.GetType(
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
        this.selectedTypeForm = this._formBuilder.group({
            typeId: [''],
            code: [null, [Validators.required]],
            label: [null, [Validators.required]],
            dureeNominal: [null],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this.types$ = this._typeService.types$;

        this._typeService.typesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.typesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getTypes();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateType() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._typeService.CreateNewType().subscribe((newType) => {
            this.selectedType = newType;
            this.selectedTypeForm.patchValue(newType);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(typeId: string): void {
        if (this.selectedType && this.selectedType.typeId === typeId) {
            this.closeDetails();
            return;
        }
        this._typeService.GetTypeById(typeId).subscribe((type) => {
            this.selectedType = type;
            this.selectedTypeForm.patchValue(type);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedType = null;
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedType(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedTypeForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const type = this.selectedTypeForm.getRawValue();

        if (type.typeId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._typeService
                .AddType(type)
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

        if (type.typeId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._typeService
                .UpdateType(type)
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

    deleteSelectedType(type: Type): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer ce type',
            message:
                'Êtes-vous sûr de vouloir supprimer ce type? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._typeService
                    .DeleteType({ typeId: type.typeId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.typeId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
