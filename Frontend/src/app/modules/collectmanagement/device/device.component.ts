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
import { Device } from '../../../core/device/device.model';
import { DeviceService } from '../../../core/device/device.service';
import { DeviceRealtimeService } from '../../../core/device/device-realtime.service';
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
    selector: 'app-device',
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
    templateUrl: './device.component.html',
    styleUrl: './device.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class DeviceComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    devices$: Observable<Device[]>;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    devicesLength: number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedDevice: Device | null = null;
    selectedDeviceForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    constructor(
        private _deviceService: DeviceService,
        private _deviceRealtimeService: DeviceRealtimeService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) {}

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        this.getDevices()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getDevices() {
        return this._deviceService.GetDevice(
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
        this.selectedDeviceForm = this._formBuilder.group({
            deviceId: [''],
            deviceName: [null, [Validators.required]],
            matricule: [null, [Validators.required]],
            nombreCapteur: [0, [Validators.required]],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        this.devices$ = this._deviceService.devices$;

        this.devices$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService.devicesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.devicesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this._deviceRealtimeService.deviceStatusChanged$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((payload) => {
                this._deviceService.applyDeviceStatusChange(payload);
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query) => {
                    this.closeDetails();
                    this.isLoading = true;
                    return this.getDevices();
                }),
                map(() => {
                    this.isLoading = false;
                })
            )
            .subscribe();
    }

    CreateDevice() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._deviceService.CreateNewDevice().subscribe((newDevice) => {
            this.selectedDevice = newDevice;
            this.selectedDeviceForm.reset(newDevice);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(deviceId: string): void {
        if (this.selectedDevice && this.selectedDevice.deviceId === deviceId) {
            this.closeDetails();
            return;
        }
        this._deviceService.GetDeviceById(deviceId).subscribe((device) => {
            this.selectedDevice = device;
            this.selectedDeviceForm.reset(device);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedDevice = null;
        this.selectedDeviceForm.reset({
            deviceId: '',
            deviceName: null,
            matricule: null,
        });
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedDevice(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedDeviceForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const device = this.selectedDeviceForm.getRawValue();

        if (device.deviceId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._deviceService
                .AddDevice(device)
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

        if (device.deviceId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._deviceService
                .UpdateDevice(device)
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

    deleteSelectedDevice(device: Device): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer ce device',
            message:
                'Êtes-vous sûr de vouloir supprimer ce device? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._deviceService
                    .DeleteDevice({ deviceId: device.deviceId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    trackByFn(index: number, item: any): any {
        return item.deviceId || index;
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
