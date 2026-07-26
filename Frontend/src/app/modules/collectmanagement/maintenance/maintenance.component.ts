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
import {
    debounceTime,
    finalize,
    map,
    merge,
    Observable,
    startWith,
    Subject,
    switchMap,
    takeUntil,
    combineLatest,
} from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { FuseConfirmationService } from '../../../../@fuse/services/confirmation';
import { Maintenance, MaintenanceRfidResponse } from '../../../core/maintenance/maintenance.model';
import { MaintenanceService } from '../../../core/maintenance/maintenance.service';
import { Employee } from '../../../core/employee/employee.model';
import { EmployeeService } from '../../../core/employee/employee.service';
import { Device } from '../../../core/device/device.model';
import { DeviceRealtimeService } from '../../../core/device/device-realtime.service';
import { DeviceService } from '../../../core/device/device.service';
import { AlerteService } from '../../../core/alerte/alerte.service';
import { TypeService } from '../../../core/type/type.service';
import { forkJoin } from 'rxjs';
import { AsyncPipe, DatePipe, NgClass, NgFor, NgIf, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMenuModule } from '@angular/material/menu';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'app-maintenance',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatOptionModule,
        MatNativeDateModule,
        MatSelectModule,
        MatDatepickerModule,
        MatMenuModule,
        MatProgressBarModule,
        MatSortModule,
        MatTooltipModule,
        NgClass,
        NgFor,
        NgIf,
        NgTemplateOutlet,
        ReactiveFormsModule,
        MatPaginatorModule,
        TranslocoDirective,
    ],
    templateUrl: './maintenance.component.html',
    styleUrl: './maintenance.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class MaintenanceComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    maintenances$: Observable<Maintenance[]>;
    employees: Employee[] = [];
    devices: Device[] = [];
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    maintenancesLength: number;
    displayedMaintenancesLength: number = 0;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedMaintenance: Maintenance | null = null;
    selectedMaintenanceForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked = false;
    roleNavigation: RoleNavigation;

    // RFID Scan properties
    rfidScanInput: UntypedFormControl = new UntypedFormControl();
    scanResult: MaintenanceRfidResponse | null = null;
    isScanning: boolean = false;
    isRealtimeConnected: boolean = false;

    // Status Filter
    statusFilter: UntypedFormControl = new UntypedFormControl('all');
    deviceHeaderFilter: UntypedFormControl = new UntypedFormControl([]);
    employeeHeaderFilter: UntypedFormControl = new UntypedFormControl([]);
    fromDateFilter: UntypedFormControl = new UntypedFormControl(null);
    toDateFilter: UntypedFormControl = new UntypedFormControl(null);

    // Alert type lookup: deviceId → type label
    private _alerteTypeLabelMap: Map<string, string> = new Map();

    constructor(
        private _maintenanceService: MaintenanceService,
        private _employeeService: EmployeeService,
        private _deviceService: DeviceService,
        private _deviceRealtimeService: DeviceRealtimeService,
        private _alerteService: AlerteService,
        private _typeService: TypeService,
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder
    ) { }

    SortChange() {
        this.closeDetails();
        this.isLoading = true;
        const currentFilter = this.statusFilter.value;
        this.getMaintenances(currentFilter)
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getMaintenances(filter: string = 'all') {
        const page = ((this._paginator?.pageIndex ?? 0) | 0) + 1;
        const size = this._paginator?.pageSize ?? 10;
        const sort = this._sort?.active ?? '';
        const order = this._sort?.direction ?? 'asc';

        return this._maintenanceService.GetMaintenance(
            page,
            size,
            sort,
            order,
            this.searchInputControl.value,
            filter,
            this.toDateParam(this.fromDateFilter.value),
            this.toDateParam(this.toDateFilter.value)
        );
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    getEmployeeName(employeeId: string): string {
        const emp = this.employees.find((e) => e.employeeId === employeeId);
        return emp ? `${emp.nom} ${emp.prenom}` : '';
    }

    getDeviceName(deviceId: string): string {
        const dev = this.devices.find((d) => d.deviceId === deviceId);
        return dev ? dev.deviceName : '';
    }

    getAlerteTypeName(deviceId: string): string {
        return this._alerteTypeLabelMap.get(deviceId) || '-';
    }

    getDuration(start?: string, end?: string): string {
        if (!start || !end) return '-';
        const startDate = new Date(start);
        const endDate = new Date(end);
        const diffMs = endDate.getTime() - startDate.getTime();
        if (diffMs < 0 || isNaN(diffMs)) return '-';
        const totalSeconds = Math.floor(diffMs / 1000);
        const days = Math.floor(totalSeconds / 86400);
        const hours = Math.floor((totalSeconds % 86400) / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        if (days > 0) return `${days}j ${hours}h ${minutes}m`;
        if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
        if (minutes > 0) return `${minutes}m ${seconds}s`;
        return `${seconds}s`;
    }

    ngOnInit(): void {
        this.selectedMaintenanceForm = this._formBuilder.group({
            maintenanceId: [''],
            deviceId: [null, [Validators.required]],
            employeeId: [null, [Validators.required]],
            t1Alerte: [null],
            t2Assignment: [null],
            t3Arrival: [null],
            t4Completion: [null],
            t5Confirmation: [null],
            t6NextAlert: [null],
            description: [null],
        });

        this._activatedRoute.data.subscribe(async (data) => {
            if (!data?.navigation) {
                return;
            }
            this.roleNavigation = data.navigation;
            this._changeDetectorRef.markForCheck();
        });

        // Initialize maintenance list with filters
        // We listen to filter changes and refetch data from the service
        this.statusFilter.valueChanges.pipe(
            takeUntil(this._unsubscribeAll),
            startWith(this.statusFilter.value)
        ).subscribe(() => {
            this.refreshMaintenancesRealtime();
        });

        this.searchInputControl.valueChanges.pipe(
            takeUntil(this._unsubscribeAll),
            debounceTime(300)
        ).subscribe(() => {
            this.refreshMaintenancesRealtime();
        });

        merge(
            this.fromDateFilter.valueChanges,
            this.toDateFilter.valueChanges
        )
            .pipe(takeUntil(this._unsubscribeAll), debounceTime(150))
            .subscribe(() => {
                this.refreshMaintenancesRealtime();
            });

        // Apply client-side filters on top of the server result page.
        this.maintenances$ = combineLatest([
            this._maintenanceService.maintenances$,
            this.deviceHeaderFilter.valueChanges.pipe(startWith(this.deviceHeaderFilter.value)),
            this.employeeHeaderFilter.valueChanges.pipe(startWith(this.employeeHeaderFilter.value)),
        ]).pipe(
            map(([maintenances, selectedDeviceIds, selectedEmployeeIds]) => {
                const deviceIds = this.normalizeSelectedValues(selectedDeviceIds);
                const employeeIds = this.normalizeSelectedValues(selectedEmployeeIds);

                const filtered = (maintenances ?? []).filter((maintenance) => {
                    if (deviceIds.length > 0 && !deviceIds.includes(maintenance.deviceId)) {
                        return false;
                    }

                    if (employeeIds.length > 0 && !employeeIds.includes(maintenance.employeeId)) {
                        return false;
                    }

                    return true;
                });

                this.displayedMaintenancesLength = filtered.length;
                return filtered;
            })
        );

        this._maintenanceService.maintenancesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.maintenancesLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this._employeeService.employees$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((employees) => {
                this.employees = employees ?? [];
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService.devices$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((devices) => {
                this.devices = devices ?? [];
                this._changeDetectorRef.markForCheck();
            });

        // Load alertes + types to build device → type label map
        forkJoin({
            alertes: this._alerteService.GetAlerte(1, 10000, '', 'desc', ''),
            types: this._typeService.GetType(1, 10000, '', 'asc', ''),
        }).pipe(takeUntil(this._unsubscribeAll)).subscribe(({ alertes: alerteResult, types: typeResult }) => {
            const typeMap = new Map<string, string>();
            for (const t of (typeResult?.types ?? [])) {
                typeMap.set(t.typeId, t.label);
            }
            this._alerteTypeLabelMap = new Map();
            for (const a of (alerteResult?.alertes ?? [])) {
                if (!this._alerteTypeLabelMap.has(a.dispositifId)) {
                    this._alerteTypeLabelMap.set(a.dispositifId, typeMap.get(a.typeId) || '');
                }
            }
            this._changeDetectorRef.markForCheck();
        });

        this._deviceRealtimeService.connect();

        this._deviceRealtimeService.isConnected$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((isConnected) => {
                this.isRealtimeConnected = isConnected;
                this._changeDetectorRef.markForCheck();
            });

        merge(
            this._deviceRealtimeService.maintenanceCaptureUpdated$,
            this._deviceRealtimeService.refreshMaintenance$
        )
            .pipe(debounceTime(150), takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this.refreshMaintenancesRealtime();
            });
    }

    CreateMaintenance() {
        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }
        this._maintenanceService.CreateNewMaintenance().subscribe((newMaintenance) => {
            this.selectedMaintenance = newMaintenance;
            this.selectedMaintenanceForm.patchValue(newMaintenance);
            this._changeDetectorRef.markForCheck();
        });
    }

    toggleDetails(maintenanceId: string): void {
        if (this.selectedMaintenance && this.selectedMaintenance.maintenanceId === maintenanceId) {
            this.closeDetails();
            return;
        }
        this._maintenanceService.GetMaintenanceById(maintenanceId).subscribe((maintenance) => {
            this.selectedMaintenance = maintenance;
            this.selectedMaintenanceForm.patchValue(maintenance);
            this._changeDetectorRef.markForCheck();
        });
    }

    closeDetails(): void {
        this.selectedMaintenance = null;
        this._changeDetectorRef.markForCheck();
    }

    SaveSelectedMaintenance(): void {
        if (
            !this.hasActionPermission(FuseNavigationAction.Edit) &&
            !this.hasActionPermission(FuseNavigationAction.Add)
        ) {
            return;
        }

        this.saveClicked = true;

        if (this.selectedMaintenanceForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(() => {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const maintenance = this.selectedMaintenanceForm.getRawValue();

        if (maintenance.maintenanceId === 'new' && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._maintenanceService
                .AddMaintenance(maintenance)
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

        if (maintenance.maintenanceId !== 'new' && this.hasActionPermission(FuseNavigationAction.Edit)) {
            this._maintenanceService
                .UpdateMaintenance(maintenance)
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

    deleteSelectedMaintenance(maintenance: Maintenance): void {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            icon: { show: false },
            title: 'Supprimer cette maintenance',
            message:
                'Êtes-vous sûr de vouloir supprimer cette maintenance? Cette action ne peut pas être annulée!',
            actions: {
                confirm: { label: 'Supprimer' },
                cancel: { label: 'Annuler' },
            },
        });

        confirmation.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this._maintenanceService
                    .DeleteMaintenance({ maintenanceId: maintenance.maintenanceId })
                    .subscribe(() => {
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    /**
     * Scans an RFID tag for the maintenance workflow.
     * Matches the RFID with an employee, finds the active maintenance,
     * and sequentially advances T1 → T2 → T3 → T4.
     */
    ScanRfid(): void {
        const rfid = this.rfidScanInput.value?.trim();
        if (!rfid) {
            return;
        }

        this.isScanning = true;
        this.scanResult = null;
        this._changeDetectorRef.markForCheck();

        this._maintenanceService.ScanRfid(rfid)
            .pipe(
                finalize(() => {
                    this.isScanning = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe((response) => {
                this.scanResult = response;
                if (response?.success) {
                    // Refresh the maintenance list to reflect updated T values
                    this.SortChange();
                }
                this.rfidScanInput.setValue('');
                this._changeDetectorRef.markForCheck();
            });
    }

    clearScanResult(): void {
        this.scanResult = null;
        this._changeDetectorRef.markForCheck();
    }

    setStatus(status: string): void {
        this.statusFilter.setValue(status);
        this._changeDetectorRef.markForCheck();
    }

    toggleDeviceHeaderFilter(deviceId: string): void {
        const current = this.normalizeSelectedValues(this.deviceHeaderFilter.value);
        const next = current.includes(deviceId)
            ? current.filter((id) => id !== deviceId)
            : [...current, deviceId];
        this.deviceHeaderFilter.setValue(next);
        this._changeDetectorRef.markForCheck();
    }

    toggleEmployeeHeaderFilter(employeeId: string): void {
        const current = this.normalizeSelectedValues(this.employeeHeaderFilter.value);
        const next = current.includes(employeeId)
            ? current.filter((id) => id !== employeeId)
            : [...current, employeeId];
        this.employeeHeaderFilter.setValue(next);
        this._changeDetectorRef.markForCheck();
    }

    clearDeviceHeaderFilter(): void {
        this.deviceHeaderFilter.setValue([]);
        this._changeDetectorRef.markForCheck();
    }

    clearEmployeeHeaderFilter(): void {
        this.employeeHeaderFilter.setValue([]);
        this._changeDetectorRef.markForCheck();
    }

    isDeviceFilterSelected(deviceId: string): boolean {
        return this.normalizeSelectedValues(this.deviceHeaderFilter.value).includes(deviceId);
    }

    isEmployeeFilterSelected(employeeId: string): boolean {
        return this.normalizeSelectedValues(this.employeeHeaderFilter.value).includes(employeeId);
    }

    hasActiveDeviceFilter(): boolean {
        return this.normalizeSelectedValues(this.deviceHeaderFilter.value).length > 0;
    }

    hasActiveEmployeeFilter(): boolean {
        return this.normalizeSelectedValues(this.employeeHeaderFilter.value).length > 0;
    }

    clearHeaderFilters(): void {
        this.deviceHeaderFilter.setValue([]);
        this.employeeHeaderFilter.setValue([]);
        this.fromDateFilter.setValue(null);
        this.toDateFilter.setValue(null);
        this._changeDetectorRef.markForCheck();
    }

    trackByFn(index: number, item: any): any {
        return item.maintenanceId || index;
    }

    private refreshMaintenancesRealtime(): void {
        const currentFilter = this.statusFilter.value;
        this.getMaintenances(currentFilter).subscribe(() => {
            this._changeDetectorRef.markForCheck();
        });
    }

    private toDateParam(value: unknown): string | null {
        if (!value) {
            return null;
        }

        const parsed = value instanceof Date ? value : new Date(String(value));
        if (!Number.isFinite(parsed.getTime())) {
            return null;
        }

        const year = parsed.getFullYear();
        const month = String(parsed.getMonth() + 1).padStart(2, '0');
        const day = String(parsed.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    private normalizeSelectedValues(value: unknown): string[] {
        if (!Array.isArray(value)) {
            return [];
        }

        return value
            .filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
            .map((item) => item.trim());
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
