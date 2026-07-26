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
import { combineLatest, debounceTime, map, Observable, startWith, Subject, switchMap, takeUntil } from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormControl,
} from '@angular/forms';
import { MaintenanceStatItem } from '../../../core/stat/stat.model';
import { StatService } from '../../../core/stat/stat.service';
import { Employee } from '../../../core/employee/employee.model';
import { EmployeeService } from '../../../core/employee/employee.service';
import { Device } from '../../../core/device/device.model';
import { DeviceService } from '../../../core/device/device.service';
import { AsyncPipe, DatePipe, DecimalPipe, NgClass, NgFor } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { fuseAnimations } from '../../../../@fuse/animations';
import { TranslocoDirective } from '@ngneat/transloco';
import { DurationPipe } from './duration.pipe';

@Component({
    selector: 'app-stat',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        DecimalPipe,
        NgFor,
        MatButtonModule,
        MatDatepickerModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatMenuModule,
        MatNativeDateModule,
        MatProgressBarModule,
        MatTooltipModule,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        TranslocoDirective,
        DurationPipe,
    ],
    templateUrl: './stat.component.html',
    styleUrl: './stat.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class StatComponent implements OnInit, OnDestroy {
    @ViewChild(MatPaginator) private _paginator: MatPaginator;

    stats$: Observable<MaintenanceStatItem[]>;
    isLoading: boolean = false;
    statsLength: number = 0;
    displayedStatsLength: number = 0;
    employees: Employee[] = [];
    devices: Device[] = [];
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    fromDateFilter: UntypedFormControl = new UntypedFormControl(null);
    toDateFilter: UntypedFormControl = new UntypedFormControl(null);
    deviceHeaderFilter: UntypedFormControl = new UntypedFormControl([]);
    employeeHeaderFilter: UntypedFormControl = new UntypedFormControl([]);
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _statService: StatService,
        private _employeeService: EmployeeService,
        private _deviceService: DeviceService,
        private _changeDetectorRef: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        this.stats$ = combineLatest([
            this._statService.stats$,
            this.deviceHeaderFilter.valueChanges.pipe(startWith(this.deviceHeaderFilter.value)),
            this.employeeHeaderFilter.valueChanges.pipe(startWith(this.employeeHeaderFilter.value)),
        ]).pipe(
            map(([stats, selectedDeviceIds, selectedEmployeeIds]) => {
                const deviceIds = this.normalizeSelectedValues(selectedDeviceIds);
                const employeeIds = this.normalizeSelectedValues(selectedEmployeeIds);

                const filtered = (stats ?? []).filter((stat) => {
                    const matchesDevice = deviceIds.length === 0
                        || (typeof stat.deviceId === 'string' && deviceIds.includes(stat.deviceId));
                    if (!matchesDevice) {
                        return false;
                    }

                    const matchesEmployee = employeeIds.length === 0
                        || (typeof stat.employeeId === 'string' && employeeIds.includes(stat.employeeId));
                    return matchesEmployee;
                });

                this.displayedStatsLength = filtered.length;
                return filtered;
            })
        );

        this._statService.statsLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.statsLength = length;
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                debounceTime(300),
                switchMap((query) => {
                    this.isLoading = true;
                    return this.getStats();
                }),
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();

        combineLatest([
            this.fromDateFilter.valueChanges.pipe(startWith(this.fromDateFilter.value)),
            this.toDateFilter.valueChanges.pipe(startWith(this.toDateFilter.value)),
        ])
            .pipe(takeUntil(this._unsubscribeAll), debounceTime(150))
            .subscribe(() => {
                this.isLoading = true;
                this.getStats()
                    .pipe(
                        map(() => {
                            this.isLoading = false;
                            this._changeDetectorRef.markForCheck();
                        })
                    )
                    .subscribe();
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

        this._employeeService.GetEmployee(1, 1000, '', 'asc', '').subscribe();
        this._deviceService.GetDevice(1, 1000, '', 'asc', '').subscribe();
    }

    getStats() {
        return this._statService.GetStats(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize ?? 10,
            this.searchInputControl.value,
            this.toDateParam(this.fromDateFilter.value),
            this.toDateParam(this.toDateFilter.value)
        );
    }

    onPageChange() {
        this.isLoading = true;
        this.getStats()
            .pipe(
                map(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    trackByFn(index: number, item: MaintenanceStatItem): any {
        return item.maintenanceId || index;
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

    hasActiveDeviceFilter(): boolean {
        return this.normalizeSelectedValues(this.deviceHeaderFilter.value).length > 0;
    }

    hasActiveEmployeeFilter(): boolean {
        return this.normalizeSelectedValues(this.employeeHeaderFilter.value).length > 0;
    }

    isDeviceFilterSelected(deviceId: string): boolean {
        return this.normalizeSelectedValues(this.deviceHeaderFilter.value).includes(deviceId);
    }

    isEmployeeFilterSelected(employeeId: string): boolean {
        return this.normalizeSelectedValues(this.employeeHeaderFilter.value).includes(employeeId);
    }

    clearAllFilters(): void {
        this.deviceHeaderFilter.setValue([]);
        this.employeeHeaderFilter.setValue([]);
        this.fromDateFilter.setValue(null);
        this.toDateFilter.setValue(null);
        this._changeDetectorRef.markForCheck();
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
}
