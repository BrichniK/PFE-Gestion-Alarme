import {
    AfterViewInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    ElementRef,
    OnDestroy,
    OnInit,
    ViewChild,
} from '@angular/core';
import { AsyncPipe, DatePipe, NgClass } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { debounceTime, interval, merge, Subject, takeUntil } from 'rxjs';
import { TranslocoDirective } from '@ngneat/transloco';
import { AlerteService } from '../../../core/alerte/alerte.service';
import { DeviceRealtimeService } from '../../../core/device/device-realtime.service';
import { MaintenanceService } from '../../../core/maintenance/maintenance.service';
import { DeviceService } from '../../../core/device/device.service';
import { Device } from '../../../core/device/device.model';
import { EmployeeService } from '../../../core/employee/employee.service';
import { Employee } from '../../../core/employee/employee.model';
import { PlanningService } from '../../../core/planning/planning.service';
import { GroupeService } from '../../../core/groupe/groupe.service';
import { Groupe } from '../../../core/groupe/groupe.model';
import { Alerte } from '../../../core/alerte/alerte.model';
import { Maintenance } from '../../../core/maintenance/maintenance.model';
import { TypeService } from '../../../core/type/type.service';
import { Type } from '../../../core/type/type.model';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Chart, ArcElement, Tooltip, Legend, DoughnutController } from 'chart.js';

Chart.register(ArcElement, Tooltip, Legend, DoughnutController);

@Component({
    selector: 'app-accueil',
    standalone: true,
    imports: [
        AsyncPipe,
        DatePipe,
        NgClass,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        RouterLink,
        TranslocoDirective,
    ],
    templateUrl: './accueil.component.html',
    styleUrl: './accueil.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccueilComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('statusChart', { static: false }) statusChartRef: ElementRef<HTMLCanvasElement>;
    private _chart: Chart<'doughnut'> | null = null;
    private _chartReady = false;
    totalAlerts = 0;
    totalMaintenances = 0;
    totalDevices = 0;
    connectedDevices = 0;
    totalEmployees = 0;
    totalAllEmployees = 0;
    todayEmployees: { employeeId: string; nom: string; prenom: string }[] = [];
    private allEmployees: Employee[] = [];
    // Simulated trend data (in a real app, this would come from the backend)
    alertsTrend = 12; // 12% increase
    maintenanceTrend = -5; // 5% decrease
    devicesTrend = 2; // 2% increase
    employeesTrend = 0; // No change

    recentAlerts: Alerte[] = [];
    recentMaintenances: Maintenance[] = [];
    todoMaintenances: Maintenance[] = [];
    doneMaintenances: Maintenance[] = [];
    diagnostiqueMaintenances: Maintenance[] = [];
    types: Type[] = [];
    private _deviceTypeLabelMap: Map<string, string> = new Map();
    now = new Date();

    // Pagination
    pageSize = 5;
    alertsPage = 0;
    todoPage = 0;
    diagPage = 0;
    reparationPage = 0;
    donePage = 0;

    readonly statusLabels = ['Affecté', 'Diagnostique', 'Réparation', 'Alertes Récentes'];
    readonly statusColors = ['#f97316', '#3b82f6', '#28834b', '#ec0c0c'];

    private _unsubscribeAll = new Subject<void>();

    constructor(
        private _alerteService: AlerteService,
        private _maintenanceService: MaintenanceService,
        private _deviceRealtimeService: DeviceRealtimeService,
        private _deviceService: DeviceService,
        private _employeeService: EmployeeService,
        private _planningService: PlanningService,
        private _groupeService: GroupeService,
        private _typeService: TypeService,
        private _changeDetectorRef: ChangeDetectorRef,
    ) { }

    ngAfterViewInit(): void {
        this._chartReady = true;
        this.updateChart();
    }

    ngOnInit(): void {
        this.loadAlerts();

        this._typeService.GetType(1, 1000).subscribe((result) => {
            this.types = result?.types ?? [];
            this._changeDetectorRef.markForCheck();
        });

        this.loadMaintenances();

        // totalAlerts is now set from the filtered (untreated) alerts above

        this._maintenanceService.maintenancesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.totalMaintenances = length ?? 0;
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService.devicesLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.totalDevices = length ?? 0;
                this._changeDetectorRef.markForCheck();
            });

        this._deviceService.devices$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((devices: Device[]) => {
                this.connectedDevices = (devices ?? []).filter((device) => device?.isOnline).length;
                this._changeDetectorRef.markForCheck();
            });

        // Fetch today's planning to get employees scheduled today
        this._employeeService.GetEmployee(1, 1000).subscribe((empResult) => {
            this.allEmployees = empResult?.employees ?? [];
            this.totalAllEmployees = this.allEmployees.length;
            this._groupeService.GetGroupe(1, 10000).subscribe((grpResult) => {
                const allGroupes: Groupe[] = grpResult?.groupes ?? [];
                this._planningService.GetPlanning(1, 1000).subscribe((paged) => {
                    const now = new Date();
                    const todayStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;

                    const matchesDate = (d: string | null | undefined): boolean => {
                        if (!d) return false;
                        const parsed = new Date(d);
                        if (isNaN(parsed.getTime())) return false;
                        const ymd = `${parsed.getFullYear()}-${String(parsed.getMonth() + 1).padStart(2, '0')}-${String(parsed.getDate()).padStart(2, '0')}`;
                        return ymd === todayStr;
                    };

                    const todayPlannings = (paged?.plannings ?? []).filter(p => {
                        if (matchesDate(p.date)) return true;
                        if (p.dates?.length) return p.dates.some(d => matchesDate(d));
                        return false;
                    });

                    // Collect unique employee IDs through groupes
                    const empIdSet = new Set<string>();
                    todayPlannings.forEach(p => {
                        (p.groupeIds ?? []).forEach(gId => {
                            const groupe = allGroupes.find(g => g.groupeId === gId);
                            if (groupe) {
                                (groupe.employeeIds ?? []).forEach(eId => empIdSet.add(String(eId)));
                            }
                        });
                    });

                    // Match against fetched employees to get nom/prenom
                    this.todayEmployees = Array.from(empIdSet)
                        .map(id => {
                            const emp = this.allEmployees.find(e => e.employeeId === id);
                            return emp ? { employeeId: emp.employeeId, nom: emp.nom, prenom: emp.prenom } : null;
                        })
                        .filter(e => e !== null);

                    this.totalEmployees = this.todayEmployees.length;
                    this._changeDetectorRef.markForCheck();
                });
            });
        });

        // Fetch a large page to compute connected devices on dashboard cards.
        this._deviceService.GetDevice(1, 10000).subscribe();

        this._deviceRealtimeService.deviceStatusChanged$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((payload) => {
                this._deviceService.applyDeviceStatusChange(payload);
                this._changeDetectorRef.markForCheck();
            });

        this._deviceRealtimeService.connect();
        merge(
            this._deviceRealtimeService.deviceCaptureStateChanged$,
            this._deviceRealtimeService.maintenanceCaptureUpdated$,
            this._deviceRealtimeService.refreshMaintenance$
        )
            .pipe(debounceTime(150), takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this.loadMaintenances();
                this.loadAlerts();
            });

        // Force a lightweight UI tick so relative durations keep increasing without page refresh.
        interval(1000)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() => {
                this.now = new Date();
                this._changeDetectorRef.markForCheck();
            });
    }

    getTypeLabelById(typeId: string): string {
        return this.types.find((type) => type.typeId === typeId)?.label ?? typeId;
    }

    getMaintenanceDeviceLabel(maintenance: Maintenance): string {
        return maintenance.deviceName || maintenance.deviceId || '-';
    }

    getMaintenanceTypeLabel(maintenance: Maintenance): string {
        const dynamicMaintenance = maintenance as unknown as {
            typeLabel?: string;
            typeName?: string;
            typeId?: string;
            deviceId?: string;
            dispositifId?: string;
        };

        const rawType = dynamicMaintenance.typeLabel ?? dynamicMaintenance.typeName ?? dynamicMaintenance.typeId;
        if (rawType) {
            return this.getTypeLabelById(rawType);
        }

        const deviceId = dynamicMaintenance.deviceId ?? dynamicMaintenance.dispositifId ?? maintenance.deviceId;
        return this._deviceTypeLabelMap.get(deviceId) ?? '-';
    }

    // Pagination helpers
    paginate<T>(items: T[], page: number): T[] {
        const start = page * this.pageSize;
        return items.slice(start, start + this.pageSize);
    }

    totalPages(items: any[]): number {
        return Math.ceil(items.length / this.pageSize);
    }

    calcDuree(date: any, durationSeconds?: number): string {
        let diff = Number.isFinite(Number(durationSeconds))
            ? Math.floor(Number(durationSeconds))
            : -1;

        if (diff < 0) {
            if (!date) return '-';
            const start = new Date(date).getTime();
            const now = Date.now();
            diff = Math.floor((now - start) / 1000);
        }

        if (diff < 0) return '-';
        const days = Math.floor(diff / 86400); diff %= 86400;
        const hours = Math.floor(diff / 3600); diff %= 3600;
        const minutes = Math.floor(diff / 60);
        const seconds = diff % 60;
        if (days > 0) return `${days}j ${hours}h ${minutes}m`;
        if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
        if (minutes > 0) return `${minutes}m ${seconds}s`;
        return `${seconds}s`;
    }

    calcDureeBetween(t1: any, t5: any): string {
        if (!t1 || !t5) return '-';
        let diff = Math.floor((new Date(t5).getTime() - new Date(t1).getTime()) / 1000);
        if (diff < 0) return '-';
        const days = Math.floor(diff / 86400); diff %= 86400;
        const hours = Math.floor(diff / 3600); diff %= 3600;
        const minutes = Math.floor(diff / 60);
        const seconds = diff % 60;
        if (days > 0) return `${days}j ${hours}h ${minutes}m`;
        if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
        if (minutes > 0) return `${minutes}m ${seconds}s`;
        return `${seconds}s`;
    }

    get paginatedAlerts(): Alerte[] { return this.paginate(this.recentAlerts, this.alertsPage); }
    get paginatedTodo(): Maintenance[] { return this.paginate(this.todoMaintenances, this.todoPage); }
    get paginatedDiag(): Maintenance[] { return this.paginate(this.diagnostiqueMaintenances, this.diagPage); }
    get paginatedReparation(): Maintenance[] { return this.paginate(this.recentMaintenances, this.reparationPage); }
    get paginatedDone(): Maintenance[] { return this.paginate(this.doneMaintenances, this.donePage); }

    get statusDistribution(): Array<{ label: string; count: number; percentage: number; color: string }> {
        const counts = [
            this.todoMaintenances.length,
            this.diagnostiqueMaintenances.length,
            this.recentMaintenances.length,
            this.recentAlerts.length,
        ];
        const total = counts.reduce((sum, value) => sum + value, 0);

        return this.statusLabels.map((label, index) => ({
            label,
            count: counts[index],
            percentage: this.computePercentage(counts[index], total),
            color: this.statusColors[index],
        }));
    }

    private computePercentage(value: number, total: number): number {
        if (total <= 0) return 0;
        return Math.round((value / total) * 1000) / 10;
    }

    private loadAlerts(): void {
        this._alerteService.GetAlerte(1, 1000).subscribe((paged) => {
            const allAlerts = paged?.alertes ?? [];

            this._deviceTypeLabelMap = new Map();
            for (const alerte of allAlerts) {
                const typeLabel = this.getTypeLabelById(alerte.typeId);
                if (typeLabel && typeLabel !== '-' && alerte.dispositifId) {
                    this._deviceTypeLabelMap.set(alerte.dispositifId, typeLabel);
                }
            }

            this.recentAlerts = allAlerts.filter(f => f.traiter == false);
            this.totalAlerts = this.recentAlerts.length;
            this.alertsPage = 0;
            this.updateChart();
            this._changeDetectorRef.markForCheck();
        });
    }

    private loadMaintenances(): void {
        // Fetch maintenances once and derive status-based lists
        this._maintenanceService.GetMaintenance(1, 100).subscribe((paged) => {
            const all = paged?.maintenances ?? [];
            // Affecté: T3 null, T4 null, T5 null
            this.todoMaintenances = all.filter(m => !m.t3Arrival && !m.t4Completion && !m.t5Confirmation);
            // Diagnostique: T3 not null, T4 null, T5 null
            this.diagnostiqueMaintenances = all.filter(m => m.t3Arrival && !m.t4Completion && !m.t5Confirmation);
            // Réparation: T3 not null, T4 not null, T5 null
            this.recentMaintenances = all.filter(m => m.t3Arrival && m.t4Completion && !m.t5Confirmation);
            // Fin: T3 not null, T4 not null, T5 not null
            this.doneMaintenances = all.filter(m => m.t3Arrival && m.t4Completion && !!m.t5Confirmation);
            // Reset pagination
            this.todoPage = 0;
            this.diagPage = 0;
            this.reparationPage = 0;
            this.donePage = 0;
            this.updateChart();
            this._changeDetectorRef.markForCheck();
        });
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next();
        this._unsubscribeAll.complete();
        this._chart?.destroy();
    }

    updateChart(): void {
        if (!this._chartReady || !this.statusChartRef?.nativeElement) return;

        const distribution = this.statusDistribution;
        const data = distribution.map((item) => item.count);
        const labels = distribution.map((item) => `${item.label}: ${item.count} (${item.percentage}%)`);

        if (this._chart) {
            this._chart.data.labels = labels;
            this._chart.data.datasets[0].data = data;
            this._chart.update();
            return;
        }

        this._chart = new Chart(this.statusChartRef.nativeElement, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: this.statusColors,
                    borderWidth: 2,
                    borderColor: '#ffffff',
                }],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '60%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 16,
                            usePointStyle: true,
                            font: { size: 12, weight: 'bold' },
                        },
                    },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const chartLabels = ctx.chart.data.labels as string[] | undefined;
                                const label = chartLabels?.[ctx.dataIndex] ?? ctx.label ?? '';
                                return ` ${label}`;
                            },
                        },
                    },
                },
            },
        });
    }
}
