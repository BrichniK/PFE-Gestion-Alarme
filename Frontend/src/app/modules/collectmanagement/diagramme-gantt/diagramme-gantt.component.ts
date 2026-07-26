import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSliderModule } from '@angular/material/slider';
import { TranslocoModule } from '@ngneat/transloco';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { MaintenanceService } from '../../../core/maintenance/maintenance.service';
import { Maintenance } from '../../../core/maintenance/maintenance.model';
import { AlerteService } from '../../../core/alerte/alerte.service';
import { Alerte } from '../../../core/alerte/alerte.model';
import { TypeService } from '../../../core/type/type.service';
import { Type as AlerteType } from '../../../core/type/type.model';
import { fuseAnimations } from '../../../../@fuse/animations';

/** Represents one phase segment between two T steps */
export interface GanttSegment {
    label: string;
    startTime: Date;
    endTime: Date;
    durationMinutes: number;
    color: string;
    stepFrom: string;
    stepTo: string;
}

/** One row in the Gantt chart */
export interface GanttRow {
    maintenanceId: string;
    deviceName: string;
    employeeName: string;
    description: string;
    t1Alerte: Date | null;
    t2Assignment: Date | null;
    t3Arrival: Date | null;
    t4Completion: Date | null;
    t5Confirmation: Date | null;
    segments: GanttSegment[];
    currentStep: string;
    totalDurationMinutes: number;
    firstTime: Date | null;
    lastTime: Date | null;
    alerteTypeLabel: string;
    alerteTypeId: string;
}

/** Multi-day page */
interface DayPage {
    date: Date;
    dateLabel: string;
    dayOfWeek: string;
    hasData: boolean;
}

@Component({
    selector: 'app-diagramme-gantt',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatFormFieldModule,
        MatSelectModule,
        MatOptionModule,
        MatCheckboxModule,
        MatDatepickerModule,
        MatInputModule,
        MatIconModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatTooltipModule,
        MatSliderModule,
        TranslocoModule,
    ],
    templateUrl: './diagramme-gantt.component.html',
    styleUrl: './diagramme-gantt.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
    providers: [DatePipe],
})
export class DiagrammeGanttComponent implements OnInit, OnDestroy {
    isLoading = false;
    hasLoaded = false; // true once the user clicks "Afficher"

    // All rows from API, and filtered/displayed rows
    allGanttRows: GanttRow[] = [];
    filteredRows: GanttRow[] = [];

    // Filters
    startDate: Date | null = null;
    endDate: Date | null = null;

    // Type d'alerte filter
    alerteTypes: AlerteType[] = [];
    selectedTypeIds: string[] = [];


    // Multi-day pagination
    dayPages: DayPage[] = [];
    currentPageIndex = 0;
    isMultiDay = false;

    // Timeline
    displayedTimeSlots: string[] = [];
    minTime = 0;
    maxTime = 0;
    private _currentStep = 60; // minutes per slot

    // Zoom
    isZoomed = false;
    zoomLevel = 0; // 0=full day, 1=hour range, 2=minute range
    zoomStartHour: number | null = null;
    zoomEndHour: number | null = null;
    zoomStartMinute: number | null = null;
    zoomEndMinute: number | null = null;
    firstClickedSlot: number | null = null;
    scrollPosition = 0;

    // Popup
    selectedSegment: GanttSegment | null = null;
    selectedRow: GanttRow | null = null;
    popupX = 0;
    popupY = 0;
    private _popupTimeout: any = null;

    // Step colors
    readonly stepColors: { [key: string]: string } = {
        'T1→T2': '#ec0c0c',
        'T2→T3': '#3B82F6',
        'T3→T4': '#F97316',
        'T4→T5': '#28834b',
    };

    readonly stepLabels: { [key: string]: string } = {
        'T1→T2': 'Alerte → Affectation',
        'T2→T3': 'Affectation → Diagnostique',
        'T3→T4': 'Diagnostique → Réparation',
        'T4→T5': 'Réparation → Fin',
    };



    private _dayNames = ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi'];
    private _destroy$ = new Subject<void>();

    // Lookup map: deviceId → alerteTypeLabel (built from alertes + types)
    private _alerteTypeLabelMap: Map<string, string> = new Map();
    // Lookup map: deviceId → typeId (built from alertes)
    private _alerteTypeIdMap: Map<string, string> = new Map();

    constructor(
        private _maintenanceService: MaintenanceService,
        private _alerteService: AlerteService,
        private _typeService: TypeService,
        private _cdr: ChangeDetectorRef,
        private _datePipe: DatePipe,
    ) {}

    ngOnInit(): void {
        // Set default date to today
        const today = new Date();
        this.startDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        this.endDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        this.generateFullDaySlots();
        this.loadAlerteTypes();
    }

    // ============= TYPE FILTER =============

    private loadAlerteTypes(): void {
        this._typeService.GetType(1, 10000, '', 'asc', '')
            .pipe(takeUntil(this._destroy$))
            .subscribe((result) => {
                this.alerteTypes = result?.types ?? [];
                this.selectedTypeIds = [];
                this._cdr.markForCheck();
            });
    }

    get allTypesSelected(): boolean {
        return this.alerteTypes.length > 0 && this.selectedTypeIds.length === this.alerteTypes.length;
    }

    get someTypesSelected(): boolean {
        return this.selectedTypeIds.length > 0 && this.selectedTypeIds.length < this.alerteTypes.length;
    }

    toggleSelectAllTypes(): void {
        if (this.allTypesSelected) {
            this.selectedTypeIds = [];
        } else {
            this.selectedTypeIds = this.alerteTypes.map(t => t.typeId);
        }
    }

    ngOnDestroy(): void {
        this._destroy$.next();
        this._destroy$.complete();
    }

    // ============= DATE VALIDATION =============

    get canDisplay(): boolean {
        return !!this.startDate && !!this.endDate;
    }

    // ============= TIME SLOT GENERATION =============

    private generateFullDaySlots(): void {
        this.displayedTimeSlots = [];
        for (let h = 0; h < 24; h++) {
            this.displayedTimeSlots.push(h.toString().padStart(2, '0') + ':00');
        }
        this._currentStep = 60;
    }

    private updateDisplayedTimeSlots(): void {
        this.displayedTimeSlots = [];

        if (this.zoomLevel === 2 && this.zoomStartHour !== null && this.zoomEndHour !== null) {
            const startTotal = this.zoomStartHour * 60 + (this.zoomStartMinute || 0);
            const endTotal = this.zoomEndHour * 60 + (this.zoomEndMinute || 59);
            const duration = endTotal - startTotal;

            let step = 1;
            if (duration <= 60) step = 5;
            else if (duration <= 120) step = 10;
            else if (duration <= 240) step = 15;
            else step = 30;
            this._currentStep = step;

            for (let m = startTotal; m <= endTotal; m += step) {
                const h = Math.floor(m / 60);
                const min = m % 60;
                this.displayedTimeSlots.push(`${h.toString().padStart(2, '0')}:${min.toString().padStart(2, '0')}`);
            }
        } else if (this.zoomLevel === 1 && this.zoomStartHour !== null && this.zoomEndHour !== null) {
            this._currentStep = 60;
            for (let h = this.zoomStartHour; h <= this.zoomEndHour; h++) {
                this.displayedTimeSlots.push(h.toString().padStart(2, '0') + ':00');
            }
        } else {
            this._currentStep = 60;
            for (let h = 0; h < 24; h++) {
                this.displayedTimeSlots.push(h.toString().padStart(2, '0') + ':00');
            }
        }
    }

    // ============= DATA LOADING (requires date) =============

    onDisplay(): void {
        if (!this.startDate || !this.endDate) return;

        this.isLoading = true;
        this.hasLoaded = false;
        this.allGanttRows = [];
        this.filteredRows = [];
        this.resetZoom();
        this._cdr.markForCheck();

        this.generateDayPages();

        // Load maintenances, alertes, and types in parallel
        forkJoin({
            maintenances: this._maintenanceService.GetMaintenance(1, 10000, '', 'desc', '', 'all'),
            alertes: this._alerteService.GetAlerte(1, 10000, '', 'desc', ''),
            types: this._typeService.GetType(1, 10000, '', 'asc', ''),
        })
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: ({ maintenances: maintResult, alertes: alerteResult, types: typeResult }) => {
                    // Build type lookup: typeId → label
                    const typesArr = typeResult?.types ?? [];
                    const typeMap = new Map<string, string>();
                    for (const t of typesArr) {
                        typeMap.set(t.typeId, t.label);
                    }

                    // Build alerte lookup: dispositifId → type label (latest alerte per device)
                    const alertesArr = alerteResult?.alertes ?? [];
                    this._alerteTypeLabelMap = new Map();
                    this._alerteTypeIdMap = new Map();
                    for (const a of alertesArr) {
                        if (!this._alerteTypeLabelMap.has(a.dispositifId)) {
                            this._alerteTypeLabelMap.set(a.dispositifId, typeMap.get(a.typeId) || '');
                            this._alerteTypeIdMap.set(a.dispositifId, a.typeId);
                        }
                    }

                    const maintenances = maintResult?.maintenances ?? [];
                    this.allGanttRows = maintenances
                        .filter((m) => m.t1Alerte)
                        .map((m) => this.processMaintenanceToRow(m));

                    // Filter by selected date range
                    const start = new Date(this.startDate!);
                    start.setHours(0, 0, 0, 0);
                    const end = new Date(this.endDate!);
                    end.setHours(23, 59, 59, 999);

                    this.allGanttRows = this.allGanttRows.filter((r) => {
                        if (!r.firstTime) return false;
                        return r.firstTime >= start && r.firstTime <= end;
                    });

                    // Filter by selected type d'alerte
                    if (this.selectedTypeIds.length > 0 && this.selectedTypeIds.length < this.alerteTypes.length) {
                        this.allGanttRows = this.allGanttRows.filter((r) =>
                            r.alerteTypeId && this.selectedTypeIds.includes(r.alerteTypeId)
                        );
                    }

                    this.markPagesWithData();
                    this.filterDataForCurrentPage();
                    this.hasLoaded = true;
                    this.isLoading = false;
                    this._cdr.markForCheck();
                },
                error: () => {
                    this.isLoading = false;
                    this.hasLoaded = true;
                    this._cdr.markForCheck();
                },
            });
    }

    // ============= MULTI-DAY PAGINATION =============

    private generateDayPages(): void {
        this.dayPages = [];
        if (!this.startDate || !this.endDate) return;
        const start = new Date(this.startDate);
        start.setHours(0, 0, 0, 0);
        const end = new Date(this.endDate);
        end.setHours(23, 59, 59, 999);

        const current = new Date(start);
        while (current <= end) {
            this.dayPages.push({
                date: new Date(current),
                dateLabel: this._datePipe.transform(current, 'dd/MM/yyyy') || '',
                dayOfWeek: this._dayNames[current.getDay()],
                hasData: false,
            });
            current.setDate(current.getDate() + 1);
        }
        this.isMultiDay = this.dayPages.length > 1;
        this.currentPageIndex = 0;
    }

    private markPagesWithData(): void {
        this.dayPages.forEach((page) => {
            const dayStart = new Date(page.date);
            dayStart.setHours(0, 0, 0, 0);
            const dayEnd = new Date(page.date);
            dayEnd.setHours(23, 59, 59, 999);

            page.hasData = this.allGanttRows.some((row) =>
                row.firstTime && row.firstTime >= dayStart && row.firstTime <= dayEnd
            );
        });
        this.dayPages = this.dayPages.filter((p) => p.hasData);
        this.isMultiDay = this.dayPages.length > 1;
        if (this.currentPageIndex >= this.dayPages.length) {
            this.currentPageIndex = 0;
        }
    }

    private filterDataForCurrentPage(): void {
        if (this.dayPages.length === 0) {
            this.filteredRows = this.allGanttRows;
            this.updateTimeRange();
            return;
        }

        const currentDay = this.dayPages[this.currentPageIndex]?.date;
        if (!currentDay) {
            this.filteredRows = [];
            return;
        }

        const dayStart = new Date(currentDay);
        dayStart.setHours(0, 0, 0, 0);
        const dayEnd = new Date(currentDay);
        dayEnd.setHours(23, 59, 59, 999);

        this.filteredRows = this.allGanttRows.filter((row) => {
            if (!row.firstTime) return false;
            return row.firstTime >= dayStart && row.firstTime <= dayEnd;
        });

        this.updateTimeRange();
    }

    private updateTimeRange(): void {
        const currentDay = this.dayPages.length > 0
            ? this.dayPages[this.currentPageIndex]?.date
            : this.startDate;

        if (currentDay) {
            const dayStart = new Date(currentDay);
            const dayEnd = new Date(currentDay);

            if (this.zoomLevel === 2 && this.zoomStartHour !== null && this.zoomEndHour !== null) {
                dayStart.setHours(this.zoomStartHour, this.zoomStartMinute || 0, 0, 0);
                dayEnd.setHours(this.zoomEndHour, this.zoomEndMinute || 59, 59, 999);
            } else if (this.zoomLevel === 1 && this.zoomStartHour !== null && this.zoomEndHour !== null) {
                dayStart.setHours(this.zoomStartHour, 0, 0, 0);
                dayEnd.setHours(this.zoomEndHour, 59, 59, 999);
            } else {
                dayStart.setHours(0, 0, 0, 0);
                dayEnd.setHours(23, 59, 59, 999);
            }

            this.minTime = dayStart.getTime();
            this.maxTime = dayEnd.getTime();
        }

        this.updateDisplayedTimeSlots();
        this._cdr.markForCheck();
    }

    goToPage(index: number): void {
        if (index < 0 || index >= this.dayPages.length) return;
        this.currentPageIndex = index;
        this.resetZoom();
        this.filterDataForCurrentPage();
        this._cdr.markForCheck();
    }

    previousPage(): void { this.goToPage(this.currentPageIndex - 1); }
    nextPage(): void { this.goToPage(this.currentPageIndex + 1); }

    get currentDayLabel(): string {
        if (this.dayPages.length === 0) return '';
        const page = this.dayPages[this.currentPageIndex];
        return page ? `${page.dayOfWeek} ${page.dateLabel}` : '';
    }

    // ============= ZOOM =============

    onTimeSlotClick(slotIndex: number): void {
        if (this.filteredRows.length === 0) return;

        if (this.firstClickedSlot === null) {
            this.firstClickedSlot = slotIndex;
            this._cdr.markForCheck();
            return;
        }

        const first = this.firstClickedSlot;
        const second = slotIndex;
        this.firstClickedSlot = null;

        const minIdx = Math.min(first, second);
        const maxIdx = Math.max(first, second);
        if (minIdx === maxIdx) return;

        if (this.zoomLevel === 0) {
            this.applyZoomLevel1(minIdx, maxIdx);
        } else if (this.zoomLevel === 1) {
            if (this.zoomStartHour === null) return;
            const startH = this.zoomStartHour + minIdx;
            const endH = this.zoomStartHour + maxIdx;
            this.applyZoomLevel2(startH, 0, endH, 59);
        } else if (this.zoomLevel === 2) {
            if (this.zoomStartHour === null || this.zoomStartMinute === null) return;
            const baseMin = this.zoomStartHour * 60 + this.zoomStartMinute;
            const startTotal = baseMin + (minIdx * this._currentStep);
            const endTotal = baseMin + (maxIdx * this._currentStep);
            this.applyZoomLevel2(
                Math.floor(startTotal / 60), startTotal % 60,
                Math.floor(endTotal / 60), endTotal % 60
            );
        }

        this._cdr.markForCheck();
    }

    private applyZoomLevel1(startHour: number, endHour: number): void {
        if (startHour === endHour) return;
        const duration = endHour - startHour;
        if (duration <= 4) {
            this.applyZoomLevel2(startHour, 0, endHour, 59);
            return;
        }
        this.isZoomed = true;
        this.zoomLevel = 1;
        this.zoomStartHour = startHour;
        this.zoomEndHour = endHour;
        this.zoomStartMinute = null;
        this.zoomEndMinute = null;
        this.scrollPosition = startHour;
        this.updateTimeRange();
    }

    private applyZoomLevel2(startH: number, startM: number, endH: number, endM: number): void {
        this.isZoomed = true;
        this.zoomLevel = 2;
        this.zoomStartHour = startH;
        this.zoomEndHour = endH;
        this.zoomStartMinute = startM;
        this.zoomEndMinute = endM;
        this.scrollPosition = startH * 60 + startM;
        this.updateTimeRange();
    }

    resetZoom(): void {
        this.isZoomed = false;
        this.zoomLevel = 0;
        this.zoomStartHour = null;
        this.zoomEndHour = null;
        this.zoomStartMinute = null;
        this.zoomEndMinute = null;
        this.firstClickedSlot = null;
        this.scrollPosition = 0;
        this.updateTimeRange();
    }

    // Scroll controls
    getScrollMin(): number { return 0; }

    getScrollMax(): number {
        if (this.zoomLevel === 1) {
            const zoomDuration = (this.zoomEndHour || 0) - (this.zoomStartHour || 0);
            return Math.max(0, 23 - zoomDuration);
        } else if (this.zoomLevel === 2) {
            const startMin = (this.zoomStartHour || 0) * 60 + (this.zoomStartMinute || 0);
            const endMin = (this.zoomEndHour || 0) * 60 + (this.zoomEndMinute || 59);
            const zoomDuration = endMin - startMin;
            return Math.max(0, 1439 - zoomDuration);
        }
        return 0;
    }

    onScrollChange(event: Event): void {
        const target = event.target as HTMLInputElement;
        const newPos = parseInt(target.value, 10);
        this.scrollPosition = newPos;
        this.applyScrollPosition();
    }

    private applyScrollPosition(): void {
        if (this.zoomLevel === 1) {
            const duration = (this.zoomEndHour || 0) - (this.zoomStartHour || 0);
            this.zoomStartHour = this.scrollPosition;
            this.zoomEndHour = Math.min(23, this.scrollPosition + duration);
        } else if (this.zoomLevel === 2) {
            const startMin = (this.zoomStartHour || 0) * 60 + (this.zoomStartMinute || 0);
            const endMin = (this.zoomEndHour || 0) * 60 + (this.zoomEndMinute || 59);
            const duration = endMin - startMin;
            const newEnd = Math.min(1439, this.scrollPosition + duration);
            this.zoomStartHour = Math.floor(this.scrollPosition / 60);
            this.zoomStartMinute = this.scrollPosition % 60;
            this.zoomEndHour = Math.floor(newEnd / 60);
            this.zoomEndMinute = newEnd % 60;
        }
        this.updateTimeRange();
    }

    getZoomLabel(): string {
        if (this.zoomStartHour !== null && this.zoomEndHour !== null) {
            if (this.zoomLevel === 2) {
                const s = `${this.zoomStartHour.toString().padStart(2, '0')}:${(this.zoomStartMinute || 0).toString().padStart(2, '0')}`;
                const e = `${this.zoomEndHour.toString().padStart(2, '0')}:${(this.zoomEndMinute || 59).toString().padStart(2, '0')}`;
                return `${s} - ${e}`;
            }
            return `${this.zoomStartHour.toString().padStart(2, '0')}:00 - ${this.zoomEndHour.toString().padStart(2, '0')}:00`;
        }
        return '';
    }

    getFirstClickLabel(): string {
        if (this.firstClickedSlot !== null && this.displayedTimeSlots[this.firstClickedSlot]) {
            return this.displayedTimeSlots[this.firstClickedSlot];
        }
        return '';
    }

    // ============= PROCESS DATA =============

    private processMaintenanceToRow(m: Maintenance): GanttRow {
        const t1 = m.t1Alerte ? new Date(m.t1Alerte) : null;
        const t2 = m.t2Assignment ? new Date(m.t2Assignment) : null;
        const t3 = m.t3Arrival ? new Date(m.t3Arrival) : null;
        const t4 = m.t4Completion ? new Date(m.t4Completion) : null;
        const t5 = m.t5Confirmation ? new Date(m.t5Confirmation) : null;

        const segments: GanttSegment[] = [];
        const steps: { from: string; to: string; start: Date | null; end: Date | null }[] = [
            { from: 'T1', to: 'T2', start: t1, end: t2 },
            { from: 'T2', to: 'T3', start: t2, end: t3 },
            { from: 'T3', to: 'T4', start: t3, end: t4 },
            { from: 'T4', to: 'T5', start: t4, end: t5 },
        ];

        for (const step of steps) {
            if (step.start && step.end) {
                const key = `${step.from}→${step.to}`;
                const durationMs = step.end.getTime() - step.start.getTime();
                segments.push({
                    label: this.stepLabels[key] || key,
                    startTime: step.start,
                    endTime: step.end,
                    durationMinutes: durationMs / 60000,
                    color: this.stepColors[key] || '#9CA3AF',
                    stepFrom: step.from,
                    stepTo: step.to,
                });
            }
        }

        let currentStep = 'T1 - Alerte';
        if (t5) currentStep = 'Terminé';
        else if (t4) currentStep = 'T5 - Fin';
        else if (t3) currentStep = 'T4 - Réparation';
        else if (t2) currentStep = 'T3 - Diagnostique';
        else if (t1) currentStep = 'T2 - Affectation';

        const allTimes = [t1, t2, t3, t4, t5].filter(Boolean) as Date[];
        const firstTime = allTimes.length > 0 ? new Date(Math.min(...allTimes.map((d) => d.getTime()))) : null;
        const lastTime = allTimes.length > 0 ? new Date(Math.max(...allTimes.map((d) => d.getTime()))) : null;
        const totalDuration = firstTime && lastTime ? (lastTime.getTime() - firstTime.getTime()) / 60000 : 0;

        return {
            maintenanceId: m.maintenanceId,
            deviceName: m.deviceName || 'N/A',
            employeeName: `${m.employeePrenom ?? ''} ${m.employeeNom ?? ''}`.trim() || 'N/A',
            description: m.description || '',
            t1Alerte: t1, t2Assignment: t2, t3Arrival: t3, t4Completion: t4, t5Confirmation: t5,
            segments, currentStep, totalDurationMinutes: totalDuration, firstTime, lastTime,
            alerteTypeLabel: this._alerteTypeLabelMap.get(m.deviceId) || '',
            alerteTypeId: this._alerteTypeIdMap.get(m.deviceId) || '',
        };
    }

    // ============= BAR POSITIONING =============

    getBarLeftPercent(segment: GanttSegment): string {
        const total = this.maxTime - this.minTime;
        if (total <= 0) return '0%';
        const offset = Math.max(segment.startTime.getTime(), this.minTime) - this.minTime;
        return Math.max(0, Math.min(100, (offset / total) * 100)) + '%';
    }

    getBarWidthPercent(segment: GanttSegment): string {
        const total = this.maxTime - this.minTime;
        if (total <= 0) return '0%';
        const clampedStart = Math.max(segment.startTime.getTime(), this.minTime);
        const clampedEnd = Math.min(segment.endTime.getTime(), this.maxTime);
        const duration = clampedEnd - clampedStart;
        return Math.max(0.3, Math.min(100, (duration / total) * 100)) + '%';
    }

    getTMarkerPercent(date: Date | null): string {
        if (!date) return '-1%';
        const total = this.maxTime - this.minTime;
        if (total <= 0) return '0%';
        const offset = date.getTime() - this.minTime;
        const pct = (offset / total) * 100;
        if (pct < 0 || pct > 100) return '-1%'; // hide if out of range
        return pct + '%';
    }

    // ============= TOOLTIP & POPUP =============

    getSegmentTooltip(segment: GanttSegment): string {
        const start = this._datePipe.transform(segment.startTime, 'dd/MM/yyyy HH:mm:ss');
        const end = this._datePipe.transform(segment.endTime, 'dd/MM/yyyy HH:mm:ss');
        return `${segment.label}\n${start} → ${end}\nDurée: ${this.formatDuration(segment.durationMinutes)}`;
    }

    showSegmentPopup(event: MouseEvent, segment: GanttSegment, row: GanttRow): void {
        if (this._popupTimeout) { clearTimeout(this._popupTimeout); this._popupTimeout = null; }
        this.selectedSegment = segment;
        this.selectedRow = row;

        const popupWidth = 350;
        const popupHeight = 500;
        const vw = window.innerWidth;
        const vh = window.innerHeight;

        this.popupX = event.clientX + 15;
        if (this.popupX + popupWidth > vw) this.popupX = event.clientX - popupWidth - 15;

        // Position above or below cursor depending on available space
        const spaceBelow = vh - event.clientY;
        const spaceAbove = event.clientY;

        if (spaceBelow >= popupHeight + 10) {
            // Enough space below
            this.popupY = event.clientY + 10;
        } else if (spaceAbove >= popupHeight + 10) {
            // Show above cursor
            this.popupY = event.clientY - popupHeight - 10;
        } else {
            // Not enough space either way — pin to top or bottom
            this.popupY = Math.max(10, vh - popupHeight - 10);
        }

        this._cdr.markForCheck();
    }

    hideSegmentPopup(): void {
        this._popupTimeout = setTimeout(() => {
            this.selectedSegment = null;
            this.selectedRow = null;
            this._cdr.markForCheck();
        }, 200);
    }

    keepPopupOpen(): void {
        if (this._popupTimeout) { clearTimeout(this._popupTimeout); this._popupTimeout = null; }
    }

    // ============= FORMAT HELPERS =============

    formatDuration(minutes: number): string {
        if (!minutes || minutes <= 0) return '-';
        const totalSeconds = Math.round(minutes * 60);
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = totalSeconds % 60;
        if (h === 0 && m === 0) return `${s}s`;
        if (h === 0) return `${m}m ${s}s`;
        if (m === 0 && s === 0) return `${h}h`;
        if (s === 0) return `${h}h ${m}m`;
        return `${h}h ${m}m ${s}s`;
    }

    /** Compute duration between début (T1) and fin (T5) in minutes; returns null if fin is missing */
    computeAlertDurationMinutes(debut: Date | null, fin: Date | null): number | null {
        if (!debut || !fin) return null;
        const diffMs = new Date(fin).getTime() - new Date(debut).getTime();
        if (diffMs <= 0) return null;
        return diffMs / 60000;
    }

    formatDateTime(date: Date | null): string {
        if (!date) return '-';
        return this._datePipe.transform(date, 'dd/MM/yyyy HH:mm:ss') || '-';
    }

    getDuration(start: Date | null, end: Date | null): string {
        if (!start || !end) return '-';
        const diffMs = new Date(end).getTime() - new Date(start).getTime();
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

    getStepBadgeColor(step: string): string {
        if (step.includes('Terminé')) return 'bg-green-100 text-green-800';
        if (step.includes('T5')) return 'bg-emerald-100 text-emerald-800';
        if (step.includes('T4')) return 'bg-blue-100 text-blue-800';
        if (step.includes('T3')) return 'bg-orange-100 text-orange-800';
        if (step.includes('T2')) return 'bg-red-100 text-red-800';
        return 'bg-gray-100 text-gray-800';
    }

    trackByFn(index: number, row: GanttRow): string {
        return row.maintenanceId;
    }

    get legendItems() {
        return Object.entries(this.stepColors).map(([key, color]) => ({
            key, color, label: this.stepLabels[key],
        }));
    }
}
