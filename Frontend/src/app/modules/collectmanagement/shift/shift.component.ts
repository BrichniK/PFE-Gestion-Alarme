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
import { Shift } from '../../../core/shift/shift.model';
import { ShiftService } from '../../../core/shift/shift.service';
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
    selector: 'app-shift',
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
    templateUrl: './shift.component.html',
    styleUrl: './shift.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class ShiftComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    shifts$: Observable<Shift[]>;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    shiftsLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedShift: Shift | null = null;
    selectedShiftForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;
    readonly hours: string[] = Array.from({ length: 24 }, (_, i) => i.toString().padStart(2, '0'));
    readonly minutes: string[] = Array.from({ length: 60 }, (_, i) => i.toString().padStart(2, '0'));

    constructor(
        private _shiftService: ShiftService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getShifts()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getShifts() {
        return this._shiftService.GetShift(
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
        this.selectedShiftForm = this._formBuilder.group({
            shiftId: [''],
            label: [null, [Validators.required]],
            startTime: [null, [Validators.required]],
            endTime: [null, [Validators.required]],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this.shifts$ = this._shiftService.shifts$;

        this._shiftService.shiftsLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.shiftsLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getShifts();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateShift() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._shiftService.CreateNewShift().subscribe((newShift) => {
            this.selectedShift = newShift;
            this.selectedShiftForm.reset(newShift);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(shiftId: string): void {
        if (this.selectedShift && this.selectedShift.shiftId === shiftId) {
            this.closeDetails();
            return;
        }
        this._shiftService.GetShiftById(shiftId).subscribe((shift) => {
            this.selectedShift = shift;
            this.selectedShiftForm.reset(shift);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedShift = null;
        this.selectedShiftForm.reset({
            shiftId: '',
            label: null,
            startTime: null,
            endTime: null,
        });
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedShift(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedShiftForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const shift = this.selectedShiftForm.getRawValue();

        if (shift.shiftId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._shiftService
                .AddShift(shift)
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

        if (shift.shiftId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._shiftService
                .UpdateShift(shift)
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

    getHourPart(field: 'startTime' | 'endTime'): string {
        const value = this.selectedShiftForm.get(field)?.value as string | null | undefined;
        const parsed = this.parseTimeString(value);
        return parsed?.hour ?? '00';
    }

    getMinutePart(field: 'startTime' | 'endTime'): string {
        const value = this.selectedShiftForm.get(field)?.value as string | null | undefined;
        const parsed = this.parseTimeString(value);
        return parsed?.minute ?? '00';
    }

    onHourPartChange(field: 'startTime' | 'endTime', hour: string): void {
        this.updateTimeField(field, hour, undefined);
    }

    onMinutePartChange(field: 'startTime' | 'endTime', minute: string): void {
        this.updateTimeField(field, undefined, minute);
    }

    private updateTimeField(field: 'startTime' | 'endTime', nextHour?: string, nextMinute?: string): void {
        const currentHour = nextHour ?? this.getHourPart(field);
        const currentMinute = nextMinute ?? this.getMinutePart(field);
        this.selectedShiftForm.patchValue({ [field]: `${currentHour}:${currentMinute}` });
    }

    private parseTimeString(value: string | null | undefined): { hour: string; minute: string } | null {
        if (!value) {
            return null;
        }

        const match = /^(\d{2}):(\d{2})/.exec(value);
        if (match) {
            return {
                hour: match[1],
                minute: match[2],
            };
        }

        return null;
    }

    deleteSelectedShift(shift: Shift): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer ce shift',
            message:
                'Êtes-vous sûr de vouloir supprimer ce shift? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._shiftService
                    .DeleteShift({ shiftId: shift.shiftId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.shiftId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
