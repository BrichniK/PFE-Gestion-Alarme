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
import { debounceTime, distinctUntilChanged, finalize, map, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { FuseConfirmationService } from '../../../../@fuse/services/confirmation';
import { SMS } from '../../../core/sms/sms.model';
import { SMSService } from '../../../core/sms/sms.service';
import { AsyncPipe, NgClass, NgForOf, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { TranslocoDirective } from '@ngneat/transloco';
import { DeviceService } from '../../../core/device/device.service';
import { Device } from '../../../core/device/device.model';
import { MatChipsModule } from '@angular/material/chips';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
    selector: 'app-sms',
    standalone: true,
    imports: [
        AsyncPipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatOptionModule,
        MatProgressBarModule,
        MatSelectModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        NgForOf,
        TranslocoDirective,
        MatChipsModule,
        MatCheckboxModule,
    ],
    templateUrl: './sms.component.html',
    styleUrl: './sms.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations
})
export class SMSComponent implements OnInit, OnDestroy {

    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    sms$: Observable<SMS[]>;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    smssLength: number = 0;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedSMS: SMS | null = null;
    selectedSMSForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roleNavigation: any;
    devices: Device[] = [];
    selectedDeviceIds: string[] = [];

    constructor(
        private _smsService: SMSService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder,
        private _deviceService: DeviceService,
    ) {
    }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getSMSs()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getSMSs() {
        return this._smsService.GetSMS(
            ((this._paginator?.pageIndex ?? 0) | 0) + 1,
            this._paginator?.pageSize ?? 10,
            this._sort?.active,
            this._sort?.direction,
            this.searchInputControl.value);
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    ngOnInit(): void {
        this.selectedSMSForm = this._formBuilder.group({
            smsId: [''],
            nomPrenom: [null, [Validators.required]],
            phoneNumber: [null, [Validators.required]],
            deviceIds: [[]]
        });

        this._activatedRoute.data
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(async (data) => {
                if (!data?.navigation) {
                    return;
                }
                this.roleNavigation = data.navigation;
                this._changeDetectorRef.markForCheck();
            });

        this.sms$ = this._smsService.smss$;

        this._smsService.smssLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.smssLength = length ?? 0;
                this._changeDetectorRef.markForCheck();
            });

        // Initial data load - defer to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => {
            this.isLoading = true;
            this._changeDetectorRef.markForCheck();
            this.getSMSs()
                .pipe(
                    takeUntil(this._unsubscribeAll),
                    finalize(() => {
                        this.isLoading = false;
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe();
        }, 0);

        this.searchInputControl.valueChanges
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getSMSs();
                }),
                takeUntil(this._unsubscribeAll),
                finalize(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();

        this._deviceService.GetDevice(1, 1000)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((result) => {
                this.devices = result?.devices ?? [];
                this._changeDetectorRef.markForCheck();
            });
    }

    CreateSMS() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }

        this._smsService.CreateNewSMS().subscribe((newSMS) => {
            this.selectedSMS = newSMS;
            this.selectedDeviceIds = [];
            this.selectedSMSForm.patchValue({
                smsId: newSMS.smsId,
                nomPrenom: newSMS.nomPrenom,
                phoneNumber: newSMS.phoneNumber,
                deviceIds: []
            });
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(smsId: string): void {
        if (this.selectedSMS && this.selectedSMS.smsId === smsId) {
            this.closeDetails();
            return;
        }

        this._smsService.GetSMSById(smsId)
            .subscribe((sms) => {
                this.selectedSMS = sms;
                this.selectedDeviceIds = sms.devices.map(d => d.deviceId);
                this.selectedSMSForm.patchValue({
                    smsId: sms.smsId,
                    nomPrenom: sms.nomPrenom,
                    phoneNumber: sms.phoneNumber,
                    deviceIds: this.selectedDeviceIds
                });
                this._changeDetectorRef.markForCheck();
            });
    }

    closeDetails(): void {
        this.selectedSMS = null;
        this.selectedDeviceIds = [];
        this._changeDetectorRef.markForCheck();
    }

    onDeviceSelectionChange(deviceId: string, checked: boolean) {
        if (checked) {
            if (!this.selectedDeviceIds.includes(deviceId)) {
                this.selectedDeviceIds.push(deviceId);
            }
        } else {
            this.selectedDeviceIds = this.selectedDeviceIds.filter(id => id !== deviceId);
        }
        this.selectedSMSForm.patchValue({ deviceIds: this.selectedDeviceIds });
    }

    isDeviceSelected(deviceId: string): boolean {
        return this.selectedDeviceIds.includes(deviceId);
    }

    SaveSelectedSMS(): void {
        if (!this.hasActionPermission(FuseNavigationAction.Edit) && !this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }

        if (this.selectedSMSForm.invalid) {
            this._changeDetectorRef.markForCheck();
            return;
        }

        const sms = {
            ...this.selectedSMSForm.getRawValue(),
            devices: this.selectedDeviceIds.map(id => this.devices.find(d => d.deviceId === id)).filter(d => d)
        };

        if (sms.smsId === "new" && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._smsService.AddSMS(sms)
                .pipe(
                    finalize(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }

        if (sms.smsId !== "new" && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._smsService.UpdateSMS(sms)
                .pipe(
                    finalize(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }
    }

    deleteSelectedSMS(sms: SMS): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: {
                show: false,
            },
            title: 'Supprimer ce SMS',
            message: 'Êtes-vous sûr de vouloir supprimer ce SMS? Cette action ne peut pas être annulée!',
            actions: {
                confirm: {
                    label: 'Supprimer'
                },
                cancel: {
                    label: 'Annuler'
                }
            }
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._smsService.DeleteSMS({ smsId: sms.smsId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.smsId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
