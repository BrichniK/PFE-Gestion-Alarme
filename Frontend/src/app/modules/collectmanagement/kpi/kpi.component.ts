import {
    AfterViewInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    ElementRef,
    NgZone,
    OnDestroy,
    QueryList,
    ViewChild,
    ViewChildren,
    ViewEncapsulation,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { StatService } from '../../../core/stat/stat.service';
import { KpiIndicatorsResponse } from '../../../core/stat/stat.model';
import { Device } from '../../../core/device/device.model';
import { DeviceService } from '../../../core/device/device.service';

/* ═══════════════  Data Models  ═══════════════ */

interface TimelineEvent {
    id: string;
    label: string;
    icon: string;
    color: string;          // circle background
    iconColor: string;      // icon colour override
}

interface MetricDef {
    id: string;
    label: string;
    fromEventId: string;
    toEventId: string;
    row: number;   // 0 = topmost (MTBF), 1 = second row
    color: string;
}

interface MetricDefinitionCard {
    code: 'MTBF' | 'MTTF' | 'MTTD' | 'MTTR';
    description: string;
}

/** Computed metric arrow ready for SVG rendering */
interface MetricLine {
    id: string;
    label: string;
    x1: number;
    x2: number;
    y: number;
    color: string;
}

/** Computed vertical guide-line segments for one circle */
interface GuideLine {
    eventId: string;
    x: number;
    topY1: number;   // top of metrics area
    topY2: number;   // just above circle
    botY1: number;   // just below circle
    botY2: number;   // bottom of diagram
}

/** Computed baseline endpoints */
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

@Component({
    selector: 'app-kpi',
    standalone: true,
    imports: [CommonModule, MatIconModule],
    templateUrl: './kpi.component.html',
    styleUrl: './kpi.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KpiComponent implements AfterViewInit, OnDestroy {

    /* ── DOM refs ── */
    @ViewChild('diagramWrapper', { static: true }) wrapperRef!: ElementRef<HTMLElement>;
    @ViewChild('metricsSvg', { static: true }) svgRef!: ElementRef<SVGElement>;
    @ViewChildren('circleRef') circleRefs!: QueryList<ElementRef<HTMLElement>>;

    /* ═══════════════  Events  ═══════════════ */
    events: TimelineEvent[] = [
        { id: 'E1', label: 'Fonctionnement\nnormal', icon: 'check_circle', color: '#16a34a', iconColor: '#ffffff' },
        { id: 'E2', label: 'Panne 1', icon: 'cancel', color: '#dc2626', iconColor: '#ffffff' },
        { id: 'E3', label: 'Affectation', icon: 'person_add', color: '#3b82f6', iconColor: '#ffffff' },
        { id: 'E4', label: 'Diagnostic', icon: 'search', color: '#f59e0b', iconColor: '#1f2937' },
        { id: 'E5', label: 'Début\nréparation', icon: 'build', color: '#6b7280', iconColor: '#ffffff' },
        { id: 'E6', label: 'Fin de\nréparation', icon: 'check_circle', color: '#22c55e', iconColor: '#ffffff' },
        { id: 'E7', label: 'Panne 2', icon: 'cancel', color: '#dc2626', iconColor: '#ffffff' },
    ];

    /* ═══════════════  Metric definitions  ═══════════════ */
    private metricDefs: MetricDef[] = [
        { id: 'mtbf', label: 'MTBF', fromEventId: 'E2', toEventId: 'E7', row: 0, color: '#374151' },
        { id: 'mttf1', label: 'MTTF', fromEventId: 'E1', toEventId: 'E2', row: 1, color: '#16a34a' },
        { id: 'mttd', label: 'MTTD', fromEventId: 'E2', toEventId: 'E5', row: 1, color: '#d97706' },
        { id: 'mttr', label: 'MTTR', fromEventId: 'E5', toEventId: 'E6', row: 1, color: '#2563eb' },
        { id: 'mttf2', label: 'MTTF', fromEventId: 'E6', toEventId: 'E7', row: 1, color: '#16a34a' },
    ];

    private phaseDefs: PhaseDef[] = [
        { id: 'phase-detection', label: 'Detection', fromEventId: 'E1', toEventId: 'E2' },
        { id: 'phase-acquitement', label: 'Aquitement', fromEventId: 'E2', toEventId: 'E3' },
        { id: 'phase-diagnostique', label: 'Diagnostique', fromEventId: 'E3', toEventId: 'E4' },
        { id: 'phase-reparation', label: 'En cours de reparation', fromEventId: 'E4', toEventId: 'E6', yOffset: -14 },
        { id: 'phase-detection-2', label: 'Detection', fromEventId: 'E6', toEventId: 'E7' },
    ];

    readonly metricDefinitionCards: MetricDefinitionCard[] = [
        {
            code: 'MTBF',
            description:
                'Temps moyen entre les d\u00e9faillances : mesure la fiabilit\u00e9 et la disponibilit\u00e9 d\u0027un \u00e9quipement.',
        },
        {
            code: 'MTTF',
            description:
                'Temps moyen avant d\u00e9faillance : dur\u00e9e moyenne de fonctionnement avant panne.',
        },
        {
            code: 'MTTD',
            description:
                'Temps moyen de d\u00e9tection : dur\u00e9e moyenne n\u00e9cessaire pour d\u00e9tecter une d\u00e9faillance apr\u00e8s son occurrence.',
        },
        {
            code: 'MTTR',
            description:
                'Temps moyen de r\u00e9paration : dur\u00e9e moyenne pour r\u00e9parer et remettre en service apr\u00e8s d\u00e9faillance.',
        },
    ];

    /* ═══════════════  Computed SVG data  ═══════════════ */
    metricLines: MetricLine[] = [];
    guideLines: GuideLine[] = [];
    phaseLabels: PhaseLabel[] = [];
    baseline: Baseline = { x1: 0, x2: 0, y: 0 };
    svgHeight = 0;
    svgWidth = 0;

    private resizeObserver!: ResizeObserver;

    /* ── Layout constants ── */
    private readonly METRIC_AREA_TOP = 10;       // px from top of wrapper
    private readonly ROW_0_Y = 32;               // MTBF row y
    private readonly ROW_1_Y = 72;               // sub-metrics row y
    private readonly GUIDE_GAP = 6;              // gap between guide line and circle edge
    private readonly ZERO_DURATION = '00:00:00:00';
    private readonly TODAY_DATE = this.getTodayDateInputValue();

    startDate = this.TODAY_DATE;
    endDate = this.TODAY_DATE;
    isLoading = false;
    devices: Device[] = [];
    selectedDeviceId = '';
    kpiIndicators: KpiIndicatorsResponse = {
        mttr: this.ZERO_DURATION,
        mttd: this.ZERO_DURATION,
        mttf: this.ZERO_DURATION,
        mtbf: this.ZERO_DURATION,
        nbAlert: 0,
        nbPannes: 0,
    };
    constructor(
        private cdr: ChangeDetectorRef,
        private ngZone: NgZone,
        private _statService: StatService,
        private _deviceService: DeviceService
    ) { }

    /* ═══════════════  Lifecycle  ═══════════════ */

    ngAfterViewInit(): void {
        // Initial compute after first paint
        requestAnimationFrame(() => this.computeOverlay());
        this.loadDevices();
        this.fetchKpiIndicators();

        // Re-compute on resize
        this.ngZone.runOutsideAngular(() => {
            this.resizeObserver = new ResizeObserver(() => {
                requestAnimationFrame(() => this.ngZone.run(() => this.computeOverlay()));
            });
            this.resizeObserver.observe(this.wrapperRef.nativeElement);
        });
    }

    ngOnDestroy(): void {
        this.resizeObserver?.disconnect();
    }

    onStartDateChange(value: string): void {
        this.startDate = value ?? '';
    }

    onEndDateChange(value: string): void {
        this.endDate = value ?? '';
    }

    onApplyDateFilter(): void {
        this.fetchKpiIndicators();
    }

    onDeviceChange(value: string): void {
        this.selectedDeviceId = value ?? '';
        this.fetchKpiIndicators();
    }

    getMetricDisplayValue(code: MetricDefinitionCard['code']): string {
        const duration = this.getMetricDurationByCode(code);
        return this.formatDurationForTable(duration);
    }

    getMetricCardClasses(code: MetricDefinitionCard['code']): string {
        switch (code) {
            case 'MTBF':
                return 'border-slate-300 bg-slate-100';
            case 'MTTF':
                return 'border-green-300 bg-green-50';
            case 'MTTD':
                return 'border-amber-300 bg-amber-50';
            case 'MTTR':
                return 'border-blue-300 bg-blue-50';
            default:
                return 'border-slate-200 bg-slate-50';
        }
    }

    getMetricDiagramLabel(metric: MetricLine): string {
        return `${metric.label} = ${this.formatDurationForDiagram(this.getMetricDurationValue(metric.id))}`;
    }

    metricLabelRectWidth(metric: MetricLine): number {
        const text = this.getMetricDiagramLabel(metric);
        return Math.max(52, Math.round(text.length * 6.5 + 18));
    }

    metricLabelRectX(metric: MetricLine): number {
        const width = this.metricLabelRectWidth(metric);
        return this.labelX(metric) - width / 2;
    }

    private getMetricDurationByCode(code: MetricDefinitionCard['code']): string {
        switch (code) {
            case 'MTTR':
                return this.kpiIndicators.mttr ?? this.ZERO_DURATION;
            case 'MTTD':
                return this.kpiIndicators.mttd ?? this.ZERO_DURATION;
            case 'MTTF':
                return this.kpiIndicators.mttf ?? this.ZERO_DURATION;
            case 'MTBF':
                return this.kpiIndicators.mtbf ?? this.ZERO_DURATION;
            default:
                return this.ZERO_DURATION;
        }
    }

    /* ═══════════════  Core computation  ═══════════════ */

    private computeOverlay(): void {
        const wrapper = this.wrapperRef.nativeElement;
        const wrapperRect = wrapper.getBoundingClientRect();
        const circles = this.circleRefs.toArray();
        if (!circles.length) return;

        // Build map: eventId → { centerX, centerY } (relative to wrapper)
        const posMap = new Map<string, { cx: number; cy: number; r: number }>();
        circles.forEach((ref, i) => {
            const el = ref.nativeElement;
            const r = el.getBoundingClientRect();
            const cx = r.left + r.width / 2 - wrapperRect.left;
            const cy = r.top + r.height / 2 - wrapperRect.top;
            const radius = r.width / 2;
            posMap.set(this.events[i].id, { cx, cy, r: radius });
        });

        // SVG dimensions — must cover full wrapper including bottom padding
        this.svgWidth = wrapperRect.width;
        this.svgHeight = wrapperRect.height;
        const svg = this.svgRef.nativeElement;
        svg.setAttribute('width', `${this.svgWidth}`);
        svg.setAttribute('height', `${this.svgHeight}`);
        svg.setAttribute('viewBox', `0 0 ${this.svgWidth} ${this.svgHeight}`);

        // ── Baseline ──
        const firstPos = posMap.get(this.events[0].id)!;
        const lastPos = posMap.get(this.events[this.events.length - 1].id)!;
        this.baseline = {
            x1: firstPos.cx - 30,
            x2: lastPos.cx + 30,
            y: firstPos.cy,
        };

        // ── Vertical guide lines ──
        this.guideLines = this.events.map(ev => {
            const pos = posMap.get(ev.id)!;
            const topY2 = pos.cy - pos.r - this.GUIDE_GAP;
            const topSegmentLength = topY2 - this.METRIC_AREA_TOP;
            const botY1 = pos.cy + pos.r + this.GUIDE_GAP;
            return {
                eventId: ev.id,
                x: pos.cx,
                topY1: this.METRIC_AREA_TOP,
                topY2,
                botY1,
                botY2: botY1 + topSegmentLength,
            };
        });

        // ── Metric arrow lines ──
        this.phaseLabels = this.phaseDefs.map((phase) => {
            const fromPos = posMap.get(phase.fromEventId);
            const toPos = posMap.get(phase.toEventId);
            return {
                id: phase.id,
                label: phase.label,
                x: ((fromPos?.cx ?? 0) + (toPos?.cx ?? 0)) / 2,
                y: this.baseline.y - 24 + (phase.yOffset ?? 0),
            };
        });

        this.metricLines = this.metricDefs.map(def => {
            const fromPos = posMap.get(def.fromEventId);
            const toPos = posMap.get(def.toEventId);
            return {
                id: def.id,
                label: def.label,
                x1: fromPos?.cx ?? 0,
                x2: toPos?.cx ?? 0,
                y: def.row === 0 ? this.ROW_0_Y : this.ROW_1_Y,
                color: def.color,
            };
        });

        this.cdr.markForCheck();
    }

    /* ═══════════════  SVG helpers for template  ═══════════════ */

    /** Left arrowhead points */
    arrowLeft(m: MetricLine): string {
        const s = 6;
        return `${m.x1 + s},${m.y - s} ${m.x1},${m.y} ${m.x1 + s},${m.y + s}`;
    }

    /** Right arrowhead points */
    arrowRight(m: MetricLine): string {
        const s = 6;
        return `${m.x2 - s},${m.y - s} ${m.x2},${m.y} ${m.x2 - s},${m.y + s}`;
    }

    /** Label x (centered between endpoints) */
    labelX(m: MetricLine): number {
        return (m.x1 + m.x2) / 2;
    }

    /** Label y (above the arrow line) */
    labelY(m: MetricLine): number {
        return m.y - 10;
    }

    /** Multi-line label support: split on \n */
    labelLines(label: string): string[] {
        return label.split('\n');
    }

    private getMetricDurationValue(metricId: string): string {
        switch (metricId) {
            case 'mtbf':
                return this.kpiIndicators.mtbf ?? this.ZERO_DURATION;
            case 'mttr':
                return this.kpiIndicators.mttr ?? this.ZERO_DURATION;
            case 'mttd':
                return this.kpiIndicators.mttd ?? this.ZERO_DURATION;
            case 'mttf1':
            case 'mttf2':
                return this.kpiIndicators.mttf ?? this.ZERO_DURATION;
            default:
                return this.ZERO_DURATION;
        }
    }

    private formatDurationForDiagram(duration: string): string {
        const parts = (duration ?? this.ZERO_DURATION).split(':');
        if (parts.length !== 4) {
            return '00HH:00MM';
        }

        const days = Number(parts[0]);
        const hours = Number(parts[1]);
        const minutes = Number(parts[2]);
        const seconds = Number(parts[3]);

        if ([days, hours, minutes, seconds].some((value) => Number.isNaN(value) || value < 0)) {
            return '00HH:00MM';
        }

        const totalHours = (days * 24) + hours;
        return `${String(totalHours).padStart(2, '0')}HH:${String(minutes).padStart(2, '0')}MM`;
    }

    private formatDurationForTable(duration: string): string {
        const parts = (duration ?? this.ZERO_DURATION).split(':');
        if (parts.length !== 4) {
            return '00 jours 00 heure 00 minute 00 seconde';
        }

        const days = Number(parts[0]);
        const hours = Number(parts[1]);
        const minutes = Number(parts[2]);
        const seconds = Number(parts[3]);

        if ([days, hours, minutes, seconds].some((value) => Number.isNaN(value) || value < 0)) {
            return '00 jours 00 heure 00 minute 00 seconde';
        }

        return `${String(days).padStart(2, '0')} jours ${String(hours).padStart(2, '0')} heure ${String(minutes).padStart(2, '0')} minute ${String(seconds).padStart(2, '0')} seconde`;
    }

    private getTodayDateInputValue(): string {
        const now = new Date();
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const day = String(now.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    private fetchKpiIndicators(): void {
        if (!this.startDate || !this.endDate || this.startDate > this.endDate) {
            this.kpiIndicators = {
                mttr: this.ZERO_DURATION,
                mttd: this.ZERO_DURATION,
                mttf: this.ZERO_DURATION,
                mtbf: this.ZERO_DURATION,
                nbAlert: 0,
                nbPannes: 0,
            };
            this.cdr.markForCheck();
            return;
        }

        this.isLoading = true;
        this._statService.GetKpiIndicators(this.startDate, this.endDate, this.selectedDeviceId || undefined).subscribe((result) => {
            this.kpiIndicators = result ?? {
                mttr: this.ZERO_DURATION,
                mttd: this.ZERO_DURATION,
                mttf: this.ZERO_DURATION,
                mtbf: this.ZERO_DURATION,
                nbAlert: 0,
                nbPannes: 0,
            };
            this.isLoading = false;
            this.cdr.markForCheck();
        });
    }

    private loadDevices(): void {
        this._deviceService.GetDevice(1, 500).subscribe((paged) => {
            this.devices = (paged?.devices ?? []).slice().sort((a, b) =>
                (a.deviceName ?? '').localeCompare(b.deviceName ?? '')
            );
            this.cdr.markForCheck();
        });
    }
}
