import {
    AfterViewInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    ElementRef,
    NgZone,
    OnDestroy,
    OnInit,
    QueryList,
    ViewChild,
    ViewChildren,
    ViewEncapsulation,
} from '@angular/core';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormGroup,
    FormsModule,
} from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MonitoringService } from '../../../core/monitoring/monitoring.service';
import { MonitoringStats } from '../../../core/monitoring/monitoring.model';
import { DeviceService } from '../../../core/device/device.service';
import { Device } from '../../../core/device/device.model';
import { StatService } from '../../../core/stat/stat.service';
import { KpiIndicatorsResponse } from '../../../core/stat/stat.model';
import { ConfigurationGeneraleService } from '../../../core/configuration-generale/configuration-generale.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslocoDirective } from '@ngneat/transloco';
import { CommonModule } from '@angular/common';
import { fuseAnimations } from '../../../../@fuse/animations';

/* ═══════════════  Diagram Data Models  ═══════════════ */

interface TimelineEvent {
    id: string;
    label: string;
    icon: string;
    color: string;
    iconColor: string;
}

interface MetricDef {
    id: string;
    label: string;
    fromEventId: string;
    toEventId: string;
    row: number;
    color: string;
}

interface MetricLine {
    id: string;
    label: string;
    x1: number;
    x2: number;
    y: number;
    color: string;
}

interface GuideLine {
    eventId: string;
    x: number;
    topY1: number;
    topY2: number;
    botY1: number;
    botY2: number;
}

interface Baseline {
    x1: number;
    x2: number;
    y: number;
}

interface PhaseDef {
    id: string;
    label: string;
    fromEventId: string;
    toEventId: string;
    yOffset?: number;
}

interface PhaseLabel {
    id: string;
    label: string;
    x: number;
    y: number;
}

interface MetricDefinitionCard {
    code: 'MTBF' | 'MTTF' | 'MTTD' | 'MTTR';
    description: string;
}

@Component({
    selector: 'app-monitoring',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatProgressSpinnerModule,
        MatSelectModule,
        TranslocoDirective,
    ],
    templateUrl: './monitoring.component.html',
    styleUrl: './monitoring.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class MonitoringComponent implements OnInit, OnDestroy, AfterViewInit {
    dateRangeForm: UntypedFormGroup;
    stats: MonitoringStats | null = null;
    isLoading = false;
    devices: Device[] = [];
    selectedDeviceId: string | null = null;

    // SVG gauge config
    readonly radius = 80;
    readonly strokeWidth = 14;
    readonly circumference = 2 * Math.PI * 80;

    gauges: { label: string; value: number; color: string; translationKey: string }[] = [];
    totalDurations = 0;
    monitoringPourcentageSurSommeDurees = true;
    coefficientGaugeD1 = 1;
    coefficientGaugeD2 = 1;
    coefficientGaugeD3 = 1;
    coefficientGaugeD4 = 1;

    /* ═══════════════  KPI Diagram  ═══════════════ */
    @ViewChild('diagramWrapper', { static: false }) wrapperRef!: ElementRef<HTMLElement>;
    @ViewChild('metricsSvg', { static: false }) svgRef!: ElementRef<SVGElement>;
    @ViewChildren('circleRef') circleRefs!: QueryList<ElementRef<HTMLElement>>;

    private readonly METRIC_AREA_TOP = 10;
    private readonly ROW_0_Y = 32;
    private readonly ROW_1_Y = 72;
    private readonly GUIDE_GAP = 6;
    private readonly ZERO_DURATION = '00:00:00:00';

    events: TimelineEvent[] = [
        { id: 'E1', label: 'Fonctionnement\nnormal', icon: 'check_circle', color: '#16a34a', iconColor: '#ffffff' },
        { id: 'E2', label: 'Panne 1', icon: 'cancel', color: '#dc2626', iconColor: '#ffffff' },
        { id: 'E3', label: 'Affectation', icon: 'person_add', color: '#3b82f6', iconColor: '#ffffff' },
        { id: 'E4', label: 'Diagnostic', icon: 'search', color: '#f59e0b', iconColor: '#1f2937' },
        { id: 'E5', label: 'Début\nréparation', icon: 'build', color: '#6b7280', iconColor: '#ffffff' },
        { id: 'E6', label: 'Fin de\nréparation', icon: 'check_circle', color: '#22c55e', iconColor: '#ffffff' },
        { id: 'E7', label: 'Panne 2', icon: 'cancel', color: '#dc2626', iconColor: '#ffffff' },
    ];

    private metricDefs: MetricDef[] = [
        { id: 'mtbf', label: 'MTBF', fromEventId: 'E2', toEventId: 'E7', row: 0, color: '#e50808' },
        { id: 'mttf1', label: 'MTTF', fromEventId: 'E1', toEventId: 'E2', row: 1, color: '#16a34a' },
        { id: 'mttd', label: 'MTTD', fromEventId: 'E2', toEventId: 'E5', row: 1, color: '#d97706' },
        { id: 'mttr', label: 'MTTR', fromEventId: 'E5', toEventId: 'E6', row: 1, color: '#2563eb' },
        { id: 'mttf2', label: 'MTTF', fromEventId: 'E6', toEventId: 'E7', row: 1, color: '#16a34a' },
    ];

    private phaseDefs: PhaseDef[] = [
        { id: 'phase-detection', label: 'Fonctionnement', fromEventId: 'E1', toEventId: 'E2' },
        { id: 'phase-detection', label: 'Detection', fromEventId: 'E2', toEventId: 'E3' },
        { id: 'phase-acquitement', label: 'Aquitement', fromEventId: 'E3', toEventId: 'E4' },
        { id: 'phase-diagnostique', label: 'Diagnostique', fromEventId: 'E4', toEventId: 'E5' },
        { id: 'phase-reparation', label: 'Reparation', fromEventId: 'E5', toEventId: 'E6' },
        { id: 'phase-detection-2', label: 'Fonctionnement', fromEventId: 'E6', toEventId: 'E7' },
    ];

    readonly metricDefinitionCards: MetricDefinitionCard[] = [
        { code: 'MTBF', description: 'Temps moyen entre les défaillances : mesure la fiabilité et la disponibilité d\'un équipement.' },
        { code: 'MTTF', description: 'Temps moyen avant défaillance : durée moyenne de fonctionnement avant panne.' },
        { code: 'MTTD', description: 'Temps moyen de détection : durée moyenne nécessaire pour détecter une défaillance après son occurrence.' },
        { code: 'MTTR', description: 'Temps moyen de réparation : durée moyenne pour réparer et remettre en service après défaillance.' },
    ];

    metricLines: MetricLine[] = [];
    guideLines: GuideLine[] = [];
    phaseLabels: PhaseLabel[] = [];
    baseline: Baseline = { x1: 0, x2: 0, y: 0 };

    kpiIndicators: KpiIndicatorsResponse = {
        mttr: this.ZERO_DURATION,
        mttd: this.ZERO_DURATION,
        mttf: this.ZERO_DURATION,
        mtbf: this.ZERO_DURATION,
        nbAlert: 0,
        nbPannes: 0,
    };

    private resizeObserver!: ResizeObserver;
    private _diagramReady = false;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _fb: UntypedFormBuilder,
        private _monitoringService: MonitoringService,
        private _deviceService: DeviceService,
        private _statService: StatService,
        private _configurationGeneraleService: ConfigurationGeneraleService,
        private _cdr: ChangeDetectorRef,
        private _ngZone: NgZone,
    ) {}

    ngOnInit(): void {
        const now = new Date();
        const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
        const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);

        this.dateRangeForm = this._fb.group({
            startDate: [startOfMonth],
            endDate: [endOfMonth],
        });

        this._deviceService.GetDevice(1, 1000)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((result) => {
                this.devices = result?.devices ?? [];
                this._cdr.markForCheck();
            });

        this._configurationGeneraleService.GetConfiguration()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((config) => {
                this.monitoringPourcentageSurSommeDurees =
                    config?.monitoringPourcentageSurSommeDurees ?? true;
                this.coefficientGaugeD1 = this.getValidCoefficient(config?.coefficientGaugeD1);
                this.coefficientGaugeD2 = this.getValidCoefficient(config?.coefficientGaugeD2);
                this.coefficientGaugeD3 = this.getValidCoefficient(config?.coefficientGaugeD3);
                this.coefficientGaugeD4 = this.getValidCoefficient(config?.coefficientGaugeD4);
                this.buildGauges();
                this._cdr.markForCheck();
            });

        this.loadStats();
    }

    ngAfterViewInit(): void {
        this._diagramReady = true;
        // Delay to allow DOM to render after stats load
        setTimeout(() => this.computeOverlay(), 100);
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
        this.resizeObserver?.disconnect();
    }

    loadStats(): void {
        const start = this.dateRangeForm.get('startDate')?.value;
        const end = this.dateRangeForm.get('endDate')?.value;

        if (!start || !end) return;

        const startStr = this.formatDate(start);
        const endStr = this.formatDate(end);

        this.isLoading = true;
        this._cdr.markForCheck();

        this._monitoringService
            .GetMonitoringStats(startStr, endStr, this.selectedDeviceId)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: (data) => {
                    this.stats = data;
                    this.buildGauges();
                    this.isLoading = false;
                    this._cdr.markForCheck();
                    // Fetch KPI indicators with the same filters
                    this.fetchKpiIndicators(startStr, endStr);
                },
                error: () => {
                    this.isLoading = false;
                    this._cdr.markForCheck();
                },
            });
    }

    private fetchKpiIndicators(startDate: string, endDate: string): void {
        this._statService
            .GetKpiIndicators(startDate, endDate, this.selectedDeviceId || undefined)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((result) => {
                this.kpiIndicators = result ?? {
                    mttr: this.ZERO_DURATION,
                    mttd: this.ZERO_DURATION,
                    mttf: this.ZERO_DURATION,
                    mtbf: this.ZERO_DURATION,
                    nbAlert: 0,
                    nbPannes: 0,
                };
                this._cdr.markForCheck();
                // Compute diagram after data + DOM ready
                setTimeout(() => {
                    this.computeOverlay();
                    this.setupResizeObserver();
                }, 50);
            });
    }

    private buildGauges(): void {
        if (!this.stats) {
            this.gauges = [];
            this.totalDurations = 0;
            return;
        }

        const d1 = this.stats.sumD1Minutes;
        const d2 = this.stats.sumD2Minutes;
        const d3 = this.stats.sumD3Minutes;
        const d4 = this.stats.sumD4Minutes;

        this.totalDurations = this.stats.sumTotalMinutes;

        this.gauges = [
            { label: 'D1', translationKey: "Durée Moyenne d'alerte (D1)", value: d1, color: '#ec0c0c' },
            { label: 'D2', translationKey: "Durée Moyenne d'attente (D2)", value: d2, color: '#3b82f6' },
            { label: 'D3', translationKey: 'Durée Moyenne de diagnostique (D3)', value: d3, color: '#ea7908' },
            { label: 'D4', translationKey: 'Durée Moyenne de réparation (D4)', value: d4, color: '#22c55e' },
        ];
    }

    getStrokeDashoffset(value: number): number {
        if (!this.stats || this.totalDurations <= 0) {
            return this.circumference;
        }
        const ratio = Math.min(value / this.totalDurations, 1);
        return this.circumference * (1 - ratio);
    }

    getTotalSegmentPath(startValue: number, segmentValue: number, totalValue: number): string {
        if (!this.stats || totalValue <= 0 || segmentValue <= 0) {
            return '';
        }

        const centerX = 100;
        const centerY = 100;
        const radius = this.radius;

        const startAngle = (startValue / totalValue) * 360 - 90;
        let sweepDeg = (segmentValue / totalValue) * 360;

        // Cap at 359.9 to avoid zero-length arc when segment ≈ 100%
        if (sweepDeg >= 360) {
            sweepDeg = 359.9;
        }

        const endAngle = startAngle + sweepDeg;

        const toRad = (deg: number) => (deg * Math.PI) / 180;

        // If sweep > 180°, split into two half-arcs for correct rendering
        if (sweepDeg > 180) {
            const midAngle = startAngle + sweepDeg / 2;
            const sx = centerX + radius * Math.cos(toRad(startAngle));
            const sy = centerY + radius * Math.sin(toRad(startAngle));
            const mx = centerX + radius * Math.cos(toRad(midAngle));
            const my = centerY + radius * Math.sin(toRad(midAngle));
            const ex = centerX + radius * Math.cos(toRad(endAngle));
            const ey = centerY + radius * Math.sin(toRad(endAngle));
            return `M ${sx} ${sy} A ${radius} ${radius} 0 0 1 ${mx} ${my} A ${radius} ${radius} 0 0 1 ${ex} ${ey}`;
        }

        const sx = centerX + radius * Math.cos(toRad(startAngle));
        const sy = centerY + radius * Math.sin(toRad(startAngle));
        const ex = centerX + radius * Math.cos(toRad(endAngle));
        const ey = centerY + radius * Math.sin(toRad(endAngle));
        const largeArcFlag = sweepDeg > 180 ? 1 : 0;
        return `M ${sx} ${sy} A ${radius} ${radius} 0 ${largeArcFlag} 1 ${ex} ${ey}`;
    }

    formatMinutes(minutes: number): string {
        if (minutes <= 0) return '0m';
        const h = Math.floor(minutes / 60);
        const m = Math.round(minutes % 60);
        if (h > 0 && m > 0) return `${h}h ${m}mn`;
        if (h > 0) return `${h}h`;
        return `${m}mn`;
    }

    getPercentage(value: number): string {
        if (!this.stats || value <= 0) {
            return '0';
        }

        const denominator = this.monitoringPourcentageSurSommeDurees
            ? this.stats.sumTotalMinutes
            : this.stats.maxGaugeMinutes;

        if (denominator <= 0) {
            return '0';
        }

        const percentage = Math.round((value / denominator) * 100);
        return percentage.toString();
    }

    private getValidCoefficient(value: unknown): number {
        const coefficient = Number(value);
        if (!Number.isFinite(coefficient) || coefficient <= 0) {
            return 1;
        }

        return coefficient;
    }

    private formatDate(date: Date): string {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    /* ═══════════════  Diagram computation  ═══════════════ */

    private setupResizeObserver(): void {
        if (this.resizeObserver || !this.wrapperRef?.nativeElement) return;
        this._ngZone.runOutsideAngular(() => {
            this.resizeObserver = new ResizeObserver(() => {
                requestAnimationFrame(() => this._ngZone.run(() => this.computeOverlay()));
            });
            this.resizeObserver.observe(this.wrapperRef.nativeElement);
        });
    }

    private computeOverlay(): void {
        if (!this._diagramReady || !this.wrapperRef?.nativeElement || !this.svgRef?.nativeElement) return;

        const wrapper = this.wrapperRef.nativeElement;
        const wrapperRect = wrapper.getBoundingClientRect();
        const circles = this.circleRefs?.toArray();
        if (!circles?.length) return;

        const posMap = new Map<string, { cx: number; cy: number; r: number }>();
        circles.forEach((ref, i) => {
            const el = ref.nativeElement;
            const r = el.getBoundingClientRect();
            const cx = r.left + r.width / 2 - wrapperRect.left;
            const cy = r.top + r.height / 2 - wrapperRect.top;
            const radius = r.width / 2;
            posMap.set(this.events[i].id, { cx, cy, r: radius });
        });

        const svgWidth = wrapperRect.width;
        const svgHeight = wrapperRect.height;
        const svg = this.svgRef.nativeElement;
        svg.setAttribute('width', `${svgWidth}`);
        svg.setAttribute('height', `${svgHeight}`);
        svg.setAttribute('viewBox', `0 0 ${svgWidth} ${svgHeight}`);

        const firstPos = posMap.get(this.events[0].id)!;
        const lastPos = posMap.get(this.events[this.events.length - 1].id)!;
        this.baseline = { x1: firstPos.cx - 30, x2: lastPos.cx + 30, y: firstPos.cy };

        this.guideLines = this.events.map(ev => {
            const pos = posMap.get(ev.id)!;
            const topY2 = pos.cy - pos.r - this.GUIDE_GAP;
            const topSegmentLength = topY2 - this.METRIC_AREA_TOP;
            const botY1 = pos.cy + pos.r + this.GUIDE_GAP;
            return { eventId: ev.id, x: pos.cx, topY1: this.METRIC_AREA_TOP, topY2, botY1, botY2: botY1 + topSegmentLength };
        });

        this.phaseLabels = this.phaseDefs.map((phase) => {
            const fromPos = posMap.get(phase.fromEventId);
            const toPos = posMap.get(phase.toEventId);
            return { id: phase.id, label: phase.label, x: ((fromPos?.cx ?? 0) + (toPos?.cx ?? 0)) / 2, y: this.baseline.y - 24 + (phase.yOffset ?? 0) };
        });

        this.metricLines = this.metricDefs.map(def => {
            const fromPos = posMap.get(def.fromEventId);
            const toPos = posMap.get(def.toEventId);
            return { id: def.id, label: def.label, x1: fromPos?.cx ?? 0, x2: toPos?.cx ?? 0, y: def.row === 0 ? this.ROW_0_Y : this.ROW_1_Y, color: def.color };
        });

        this._cdr.markForCheck();
    }

    /* ═══════════════  SVG helpers for diagram template  ═══════════════ */

    arrowLeft(m: MetricLine): string {
        const s = 6;
        return `${m.x1 + s},${m.y - s} ${m.x1},${m.y} ${m.x1 + s},${m.y + s}`;
    }

    arrowRight(m: MetricLine): string {
        const s = 6;
        return `${m.x2 - s},${m.y - s} ${m.x2},${m.y} ${m.x2 - s},${m.y + s}`;
    }

    labelX(m: MetricLine): number {
        return (m.x1 + m.x2) / 2;
    }

    labelY(m: MetricLine): number {
        return m.y - 10;
    }

    labelLines(label: string): string[] {
        return label.split('\n');
    }

    getMetricDiagramLabel(metric: MetricLine): string {
        return `${metric.label} = ${this.formatDurationForDiagram(this.getMetricDurationValue(metric.id))}`;
    }

    metricLabelRectWidth(metric: MetricLine): number {
        const text = this.getMetricDiagramLabel(metric);
        return Math.max(52, Math.round(text.length * 6.5 + 18));
    }

    metricLabelRectX(metric: MetricLine): number {
        return this.labelX(metric) - this.metricLabelRectWidth(metric) / 2;
    }

    getMetricDisplayValue(code: MetricDefinitionCard['code']): string {
        return this.formatDurationForTable(this.getMetricDurationByCode(code));
    }

    getMetricCardClasses(code: MetricDefinitionCard['code']): string {
        switch (code) {
            case 'MTBF': return 'border-red-300 bg-red-100';
            case 'MTTF': return 'border-green-300 bg-green-50';
            case 'MTTD': return 'border-amber-300 bg-amber-50';
            case 'MTTR': return 'border-blue-300 bg-blue-50';
            default: return 'border-slate-200 bg-slate-50';
        }
    }

    private getMetricDurationByCode(code: MetricDefinitionCard['code']): string {
        switch (code) {
            case 'MTTR': return this.kpiIndicators.mttr ?? this.ZERO_DURATION;
            case 'MTTD': return this.kpiIndicators.mttd ?? this.ZERO_DURATION;
            case 'MTTF': return this.kpiIndicators.mttf ?? this.ZERO_DURATION;
            case 'MTBF': return this.kpiIndicators.mtbf ?? this.ZERO_DURATION;
            default: return this.ZERO_DURATION;
        }
    }

    private getMetricDurationValue(metricId: string): string {
        switch (metricId) {
            case 'mtbf': return this.kpiIndicators.mtbf ?? this.ZERO_DURATION;
            case 'mttr': return this.kpiIndicators.mttr ?? this.ZERO_DURATION;
            case 'mttd': return this.kpiIndicators.mttd ?? this.ZERO_DURATION;
            case 'mttf1':
            case 'mttf2': return this.kpiIndicators.mttf ?? this.ZERO_DURATION;
            default: return this.ZERO_DURATION;
        }
    }

    private formatDurationForDiagram(duration: string): string {
        const parts = (duration ?? this.ZERO_DURATION).split(':');
        if (parts.length !== 4) return '00HH:00MM';
        const days = Number(parts[0]);
        const hours = Number(parts[1]);
        const minutes = Number(parts[2]);
        const seconds = Number(parts[3]);
        if ([days, hours, minutes, seconds].some(v => Number.isNaN(v) || v < 0)) return '00HH:00MM';
        const totalHours = (days * 24) + hours;
        return `${String(totalHours).padStart(2, '0')}HH:${String(minutes).padStart(2, '0')}MM`;
    }

    private formatDurationForTable(duration: string): string {
        const parts = (duration ?? this.ZERO_DURATION).split(':');
        if (parts.length !== 4) return '00 jours 00 heure 00 minute 00 seconde';
        const days = Number(parts[0]);
        const hours = Number(parts[1]);
        const minutes = Number(parts[2]);
        const seconds = Number(parts[3]);
        if ([days, hours, minutes, seconds].some(v => Number.isNaN(v) || v < 0)) return '00 jours 00 heure 00 minute 00 seconde';
        return `${String(days).padStart(2, '0')} jours ${String(hours).padStart(2, '0')} heure ${String(minutes).padStart(2, '0')} minute ${String(seconds).padStart(2, '0')} seconde`;
    }
}
