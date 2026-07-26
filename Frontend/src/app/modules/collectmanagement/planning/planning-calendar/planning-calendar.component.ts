import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { CommonModule, DatePipe, NgClass, NgIf } from '@angular/common';
import { TranslocoDirective } from '@ngneat/transloco';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { PlanningDialogComponent } from '../planning-dialog/planning-dialog.component';
import { fuseAnimations } from '../../../../../@fuse/animations';
import { PlanningService } from '../../../../core/planning/planning.service';
import { GroupeService } from '../../../../core/groupe/groupe.service';
import { ShiftService } from '../../../../core/shift/shift.service';
import { Groupe } from '../../../../core/groupe/groupe.model';
import { Shift } from '../../../../core/shift/shift.model';
import { Planning } from '../../../../core/planning/planning.model';
import { Device } from '../../../../core/device/device.model';
import { Observable, Subject, takeUntil } from 'rxjs';
import { FuseNavigationAction } from '../../../../../@fuse/components/navigation';
import { RoleNavigation } from '../../../../core/role-utilisateur/role-utilisateur.model';
import { ActivatedRoute } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JourFerie } from '../../../../core/jour-ferie/jour-ferie.model';
import { JourFerieService } from '../../../../core/jour-ferie/jour-ferie.service';
import { Employee } from '../../../../core/employee/employee.model';

interface CalendarDay {
    date: Date;
    isCurrentMonth: boolean;
    plannings: PlanningAssignment[];
}

interface PlanningAssignment {
    id: string;
    assignmentMode: 'group' | 'employee';
    groupeIds: string[];
    employeeIds: string[];
    groupeColors: string[];
    shiftIds: string[];
    deviceIds: string[];
    groupeLabel: string;
    shiftLabel: string;
}

interface HoveredPlanningItem {
    id: string;
    label: string;
    planning: PlanningAssignment;
    employeeEntries: { name: string; deviceName: string }[];
}

@Component({
    selector: 'app-planning-calendar',
    standalone: true,
    imports: [
        CommonModule,
        TranslocoDirective,
        MatIconModule,
        MatButtonModule,
        MatProgressBarModule,
        DatePipe,
        MatTooltipModule,
        MatDialogModule,
    ],
    templateUrl: './planning-calendar.component.html',
    styleUrl: './planning-calendar.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class PlanningCalendarComponent implements OnInit, OnDestroy {
    groupes: Groupe[] = [];
    shifts: Shift[] = [];
    devices: Device[] = [];
    holidays: JourFerie[] = [];
    employees: Employee[] = [];
    plannings$: Observable<Planning[]>;

    currentDate: Date = new Date();
    currentYear: number = new Date().getFullYear();

    // Structure: Array of 12 months, each having a name and an array of days
    yearMonths: { name: string; days: CalendarDay[] }[] = [];

    weekDays: string[] = ['L', 'M', 'M', 'J', 'V', 'S', 'D']; // French initials starting Monday
    monthNames: string[] = [
        'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin',
        'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'
    ];

    isLoading: boolean = false;
    hoveredAssignments: string[] = [];
    hoveredPlanningItems: HoveredPlanningItem[] = [];
    hoveredDay: CalendarDay | null = null;
    showAffectedPopup: boolean = false;
    hoveredHolidayLabel: string = '';
    affectedPopupX: number = 0;
    affectedPopupY: number = 0;
    isMultiSelectMode: boolean = false;
    private _selectedDayKeys: Set<number> = new Set<number>();
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roleNavigation: RoleNavigation;
    private readonly _onPreviousYearHandler = () => this._onPreviousYear();
    private readonly _onNextYearHandler = () => this._onNextYear();
    private readonly _onGoToTodayHandler = () => this._onGoToToday();
    private _hidePopupTimer: ReturnType<typeof setTimeout> | null = null;

    constructor(
        private _planningService: PlanningService,
        private _jourFerieService: JourFerieService,
        private _groupeService: GroupeService,
        private _shiftService: ShiftService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _activatedRoute: ActivatedRoute,
        private _dialog: MatDialog
    ) { }

    ngOnInit(): void {
        // Listen for navigation events from parent component
        document.addEventListener('planningPreviousMonth', this._onPreviousYearHandler);
        document.addEventListener('planningNextMonth', this._onNextYearHandler);
        document.addEventListener('planningGoToToday', this._onGoToTodayHandler);

        // Get resolver data
        this._activatedRoute.data
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data) => {
                if (data?.navigation) {
                    this.roleNavigation = data.navigation;
                }
                if (data?.groupes) {
                    this.groupes = data.groupes.groupes || [];
                }
                if (data?.shifts) {
                    this.shifts = data.shifts.shifts || [];
                }
                if (data?.devices) {
                    this.devices = data.devices.devices || [];
                }
                if (data?.holidays) {
                    this.holidays = data.holidays.joursFeries || [];
                }
                if (data?.employees) {
                    this.employees = data.employees.employees || [];
                }
                if (data?.plannings?.plannings) {
                    this.generateYearCalendar();
                    this.populatePlannings(data.plannings.plannings);
                }
                this._changeDetectorRef.markForCheck();
            });

        // Load all plannings (assuming the service returns a large enough dataset for now, or we might need to trigger a large fetch)
        // ideally we should fetch by year range, but relying on existing service behavior for now
        this.plannings$ = this._planningService.plannings$;
        this.plannings$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((plannings) => {
                this.generateYearCalendar();
                this.populatePlannings(plannings);
                this._changeDetectorRef.markForCheck();
            });

        this.generateYearCalendar();
        this.refreshData();
    }

    private _onPreviousYear(): void {
        this.currentYear--;
        this.generateYearCalendar();
        this.refreshData(); // Trigger data reload if needed
    }

    private _onNextYear(): void {
        this.currentYear++;
        this.generateYearCalendar();
        this.refreshData();
    }

    private _onGoToToday(): void {
        const today = new Date();
        this.currentYear = today.getFullYear();
        this.generateYearCalendar();
        this.refreshData();

        // Scroll to today
        this.scrollToToday();
    }

    scrollToToday(): void {
        setTimeout(() => {
            const now = new Date();
            // Match the time component used in generateYearCalendar (default constructor usually sets time to 00:00:00 local if year/month/day provided)
            // But let's be safe and use setHours(0,0,0,0) if needed, but 'new Date(y, m, d)' does it.
            const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
            const id = 'day-' + today.getTime();
            const element = document.getElementById(id);
            if (element) {
                element.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'center' });
                // Optional: Flash effect or something via class toggle if desired, but user asked for border black which is static via ngClass.
            }
        }, 100); // Small delay to allow DOM to update
    }

    refreshData(): void {
        // Here we would ideally call the service to fetch data for this.currentYear
        // For now we re-trigger the observable flow if data is already there or fetch again
        this._planningService.GetPlanning(1, 10000).subscribe();
        this._jourFerieService.GetJourFerie(1, 10000).subscribe((result) => {
            this.holidays = result?.joursFeries ?? [];
            this._changeDetectorRef.markForCheck();
        });
        this._changeDetectorRef.markForCheck();
    }

    hasActionPermission(action: FuseNavigationAction): boolean {
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    generateYearCalendar(): void {
        this.yearMonths = [];

        for (let monthIndex = 0; monthIndex < 12; monthIndex++) {
            const days: CalendarDay[] = [];
            const lastDayOfMonth = new Date(this.currentYear, monthIndex + 1, 0);

            // Days of the month
            for (let day = 1; day <= lastDayOfMonth.getDate(); day++) {
                days.push({
                    date: new Date(this.currentYear, monthIndex, day),
                    isCurrentMonth: true,
                    plannings: []
                });
            }

            this.yearMonths.push({
                name: this.monthNames[monthIndex],
                days: days
            });
        }
    }

    populatePlannings(plannings: Planning[]): void {
        if (!plannings) return;

        // Clear existing
        this.yearMonths.forEach(m => m.days.forEach(d => d.plannings = []));

        plannings.forEach(planning => {
            const pDate = this._parseLocalDate(planning.date);
            if (pDate.getFullYear() === this.currentYear) {
                const monthIndex = pDate.getMonth();
                const dayOfMonth = pDate.getDate();

                // Find the day cell
                const monthObj = this.yearMonths[monthIndex];
                const dayObj = monthObj?.days.find(d => d.isCurrentMonth && d.date && d.date.getDate() === dayOfMonth);

                if (dayObj) {
                    const assignmentMode = planning.assignmentMode === 'employee' ? 'employee' : 'group';
                    const planningGroupeIds = (planning.groupeIds ?? []).map((id) => String(id));
                    const planningEmployeeIds = (planning.employeeIds ?? []).map((id) => String(id));
                    const planningShiftIds = (planning.shiftIds ?? []).map((id) => String(id));

                    const employeeLabelFromDirectory = this.employees
                        .filter((employee) => planningEmployeeIds.includes(String(employee.employeeId)))
                        .map((employee) => `${employee.prenom} ${employee.nom}`.trim())
                        .filter((name) => name.length > 0)
                        .join(', ');
                    const groupeLabelFromDirectory = this.groupes
                        .filter((g) => planningGroupeIds.includes(String(g.groupeId)))
                        .map((g) => (g.nom ?? '').trim())
                        .filter((name) => name.length > 0)
                        .join(', ');
                    const groupeLabelFromPlanning = (planning.groupes ?? [])
                        .map((g) => (g.groupeNom ?? '').trim())
                        .filter((name) => name.length > 0)
                        .join(', ');
                    const groupeLabel =
                        (assignmentMode === 'employee' ? (employeeLabelFromDirectory || planningEmployeeIds.join(', ')) : '') ||
                        groupeLabelFromDirectory ||
                        groupeLabelFromPlanning ||
                        planningGroupeIds.join(', ');

                    const shiftLabelFromDirectory = this.shifts
                        .filter((s) => planningShiftIds.includes(String(s.shiftId)))
                        .map((s) => (s.label ?? '').trim())
                        .filter((label) => label.length > 0)
                        .join(', ');
                    const shiftLabelFromPlanning = (planning.shifts ?? [])
                        .map((s) => (s.shiftLabel ?? '').trim())
                        .filter((label) => label.length > 0)
                        .join(', ');
                    const shiftLabel =
                        shiftLabelFromDirectory ||
                        shiftLabelFromPlanning ||
                        planningShiftIds.join(', ');

                    const groupeColorsFromDirectory = this.groupes
                        .filter((g) => planningGroupeIds.includes(String(g.groupeId)))
                        .map((g) => g.color)
                        .filter((c) => !!c);
                    const groupeColorsFromApi = (planning.groupeColors ?? []).filter((c) => !!c);
                    const groupeColors = groupeColorsFromDirectory.length > 0
                        ? groupeColorsFromDirectory
                        : groupeColorsFromApi;

                    dayObj.plannings.push({
                        id: planning.planningId,
                        assignmentMode,
                        groupeIds: planningGroupeIds,
                        employeeIds: planningEmployeeIds,
                        groupeColors,
                        shiftIds: planningShiftIds,
                        deviceIds: planning.deviceIds ?? [],
                        groupeLabel,
                        shiftLabel,
                    });
                }
            }
        });
    }

    isToday(date: Date): boolean {
        if (!date) return false;
        const today = new Date();
        return date.getDate() === today.getDate() &&
            date.getMonth() === today.getMonth() &&
            date.getFullYear() === today.getFullYear();
    }

    isHoliday(date: Date): boolean {
        if (!date || !this.holidays?.length) {
            return false;
        }
        const checkDate = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
        return this.holidays.some((holiday) => {
            const holidayDate = this._parseLocalDate(holiday.date);
            return new Date(holidayDate.getFullYear(), holidayDate.getMonth(), holidayDate.getDate()).getTime() === checkDate;
        });
    }

    hasWeekSeparator(date: Date): boolean {
        if (!date) return false;
        return date.getDay() === 1 && date.getDate() !== 1;
    }

    getAffectedAssignments(day: CalendarDay): string[] {
        if (!day?.plannings?.length) {
            return [];
        }

        return day.plannings
            .map((planning) => {
                const groupe = (planning.groupeLabel || '').trim();
                const shift = (planning.shiftLabel || '').trim();

                if (groupe && shift) {
                    return `${groupe} - ${shift}`;
                }
                if (groupe) {
                    return groupe;
                }
                if (shift) {
                    return shift;
                }
                return '';
            })
            .filter((line) => line.length > 0);
    }

    onDayMouseEnter(day: CalendarDay, event: MouseEvent): void {
        this._clearPopupHideTimer();
        const holidayLabel = this.getHolidayLabel(day.date);
        const hasAssignments = (day?.plannings?.length ?? 0) > 0;

        if (!hasAssignments && !holidayLabel) {
            this.onDayMouseLeave();
            return;
        }

        this.hoveredPlanningItems = hasAssignments ? this._buildHoveredPlanningItems(day) : [];
        this.hoveredAssignments = this.hoveredPlanningItems.map((item) => item.label);
        this.hoveredHolidayLabel = holidayLabel;
        this.hoveredDay = day;
        this.showAffectedPopup = true;
        this._updateAffectedPopupPosition(event);
    }

    onDayMouseMove(event: MouseEvent): void {
        if (!this.showAffectedPopup) {
            return;
        }
        this._updateAffectedPopupPosition(event);
    }

    onDayMouseLeave(): void {
        this._clearPopupHideTimer();
        this._hidePopupTimer = setTimeout(() => this._hidePopupNow(), 320);
    }

    onPopupMouseEnter(): void {
        this._clearPopupHideTimer();
    }

    onPopupMouseLeave(): void {
        this._hidePopupNow();
    }

    onPopupMouseDown(event: MouseEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }

    onPopupAddAffectation(event: MouseEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this._clearPopupHideTimer();

        if (!this.hasActionPermission(FuseNavigationAction.Add)) {
            return;
        }

        const selectedDay = this.hoveredDay;
        if (!selectedDay?.date) {
            return;
        }

        this.openPlanningDialog(selectedDay, undefined, null, true, [], [], []);
        this._hidePopupNow();
    }

    onPopupPlanningClick(item: HoveredPlanningItem, event: MouseEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this._clearPopupHideTimer();

        const selectedDay = this.hoveredDay;
        if (!selectedDay?.date) {
            return;
        }
        const selectedPlanningSnapshot = item?.planning ?? null;
        const preselectedGroupeIds =
            selectedPlanningSnapshot?.groupeIds?.length
                ? [...selectedPlanningSnapshot.groupeIds]
                : this._resolveGroupeIdsByLabel(selectedPlanningSnapshot?.groupeLabel);
        const preselectedShiftIds =
            selectedPlanningSnapshot?.shiftIds?.length
                ? [...selectedPlanningSnapshot.shiftIds]
                : this._resolveShiftIdsByLabel(selectedPlanningSnapshot?.shiftLabel);
        const preselectedDeviceIds = selectedPlanningSnapshot?.deviceIds?.length
            ? [...selectedPlanningSnapshot.deviceIds]
            : [];

        this.openPlanningDialog(
            selectedDay,
            item?.id || undefined,
            selectedPlanningSnapshot,
            false,
            preselectedGroupeIds,
            preselectedShiftIds,
            preselectedDeviceIds
        );
        this._hidePopupNow();
    }

    getPopupEmployeesText(planning: PlanningAssignment): string {
        const employees = this._resolveEmployeeNames(planning);
        if (employees.length === 0) {
            return 'Employe inconnu';
        }
        return employees.join(', ');
    }

    getPopupShiftText(planning: PlanningAssignment): string {
        const shifts = this._resolveShiftNames(planning);
        if (shifts.length === 0) {
            return 'Shift inconnu';
        }
        return shifts.join(', ');
    }

    onDayClick(day: CalendarDay, event: MouseEvent): void {
        if (this.isMultiSelectMode) {
            event.preventDefault();
            event.stopPropagation();
            this.toggleDaySelection(day);
            return;
        }

        // Always allow opening a new planning dialog, even if popup is showing
        this._hidePopupNow();
        this.openPlanningDialog(day);
    }

    toggleMultiSelectMode(): void {
        this.isMultiSelectMode = !this.isMultiSelectMode;
        if (!this.isMultiSelectMode) {
            this.clearSelectedDays();
        }
    }

    get selectedDates(): Date[] {
        return Array.from(this._selectedDayKeys)
            .map((key) => new Date(key))
            .sort((a, b) => a.getTime() - b.getTime());
    }

    isDaySelected(day: CalendarDay): boolean {
        return this._selectedDayKeys.has(day.date.getTime());
    }

    toggleDaySelection(day: CalendarDay): void {
        const key = day.date.getTime();
        if (this._selectedDayKeys.has(key)) {
            this._selectedDayKeys.delete(key);
        } else {
            this._selectedDayKeys.add(key);
        }
        this._changeDetectorRef.markForCheck();
    }

    clearSelectedDays(): void {
        if (this._selectedDayKeys.size === 0) {
            return;
        }
        this._selectedDayKeys.clear();
        this._changeDetectorRef.markForCheck();
    }

    openMultiPlanningDialog(): void {
        const dates = this.selectedDates;
        if (dates.length === 0) {
            return;
        }

        const anchorDay: CalendarDay = {
            date: dates[0],
            isCurrentMonth: true,
            plannings: [],
        };

        this.openPlanningDialog(
            anchorDay,
            undefined,
            null,
            true,
            [],
            [],
            [],
            dates
        );
    }

    onPopupAddNewClick(event: MouseEvent): void {
        event.preventDefault();
        event.stopPropagation();

        const selectedDay = this.hoveredDay;
        if (!selectedDay?.date) {
            return;
        }

        this.openPlanningDialog(selectedDay, undefined, null, true);
        this._hidePopupNow();
    }

    onPopupPlanningDelete(planningId: string, event: MouseEvent): void {
        event.preventDefault();
        event.stopPropagation();

        if (!this.hasActionPermission(FuseNavigationAction.Delete)) {
            return;
        }

        const confirmed = window.confirm('Supprimer cette affectation ?');
        if (!confirmed) {
            return;
        }

        this._planningService.DeletePlanning({ planningId }).subscribe((deleted) => {
            if (!deleted) {
                return;
            }

            if (this.hoveredDay) {
                this.hoveredDay.plannings = this.hoveredDay.plannings.filter((p) => p.id !== planningId);
                this.hoveredPlanningItems = this.hoveredPlanningItems.filter((item) => item.id !== planningId);
                this.hoveredAssignments = this.hoveredPlanningItems.map((item) => item.label);
                if (this.hoveredPlanningItems.length === 0 && !this.hoveredHolidayLabel) {
                    this._hidePopupNow();
                }
            }

            this.refreshData();
            this._changeDetectorRef.markForCheck();
        });
    }

    private _hidePopupNow(): void {
        this.showAffectedPopup = false;
        this.hoveredAssignments = [];
        this.hoveredPlanningItems = [];
        this.hoveredHolidayLabel = '';
        this.hoveredDay = null;
    }

    openPlanningDialog(
        day: CalendarDay | null,
        selectedPlanningId?: string,
        selectedPlanningSnapshot?: PlanningAssignment | null,
        forceNew: boolean = true,
        preselectedGroupeIds: string[] = [],
        preselectedShiftIds: string[] = [],
        preselectedDeviceIds: string[] = [],
        selectedDates: Date[] = []
    ): void {
        if (
            !day?.date ||
            (!this.hasActionPermission(FuseNavigationAction.Add) &&
                !this.hasActionPermission(FuseNavigationAction.Edit))
        ) {
            return;
        }

        const dialogRef = this._dialog.open(PlanningDialogComponent, {
            panelClass: 'planning-dialog',
            data: {
                forceNew,
                selectedPlanningId,
                selectedPlanningSnapshot,
                preselectedGroupeIds,
                preselectedShiftIds,
                preselectedDeviceIds,
                date: day.date,
                selectedDates,
                existingPlannings: day.plannings, // Pass all if multiple allowed, or logic to select specific
                groupes: this.groupes,
                employees: this.employees,
                shifts: this.shifts,
                devices: this.devices
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.refreshData();
                if (this._selectedDayKeys.size > 0) {
                    this.clearSelectedDays();
                }
            }
        });
    }

    getShiftColor(shiftLabel: string): string {
        // Simplified discrete colors for print look
        // Using tailwind classes or hex
        const colors = [
            '#e2e8f0', // slate-200
            '#bfdbfe', // blue-200
            '#bbf7d0', // green-200
            '#fef08a', // yellow-200
            '#fed7aa', // orange-200
            '#e9d5ff', // purple-200
        ];
        // Simple hash
        let hash = 0;
        for (let i = 0; i < shiftLabel.length; i++) {
            hash = shiftLabel.charCodeAt(i) + ((hash << 5) - hash);
        }
        const index = Math.abs(hash) % colors.length;
        return colors[index];
    }

    getDayGroupeColors(day: CalendarDay): string[] {
        if (!day?.plannings?.length) {
            return [];
        }
        const colors: string[] = [];
        day.plannings.forEach((p) => {
            if (p.assignmentMode === 'employee') {
                return;
            }
            (p.groupeColors ?? []).forEach((c) => {
                if (c && !colors.includes(c)) {
                    colors.push(c);
                }
            });
        });
        return colors;
    }

    hasEmployeeAssignment(day: CalendarDay): boolean {
        return (day?.plannings ?? []).some((planning) => planning.assignmentMode === 'employee');
    }

    getGroupeColorForPlanning(planning: PlanningAssignment): string[] {
        return (planning.groupeColors ?? []).filter((c) => !!c);
    }

    private _parseLocalDate(value: string): Date {
        // Parse YYYY-MM-DD as local date to avoid timezone day-shift.
        const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
        if (match) {
            const year = Number(match[1]);
            const month = Number(match[2]) - 1;
            const day = Number(match[3]);
            return new Date(year, month, day);
        }

        const parsed = new Date(value);
        return new Date(parsed.getFullYear(), parsed.getMonth(), parsed.getDate());
    }

    private _formatPlanningLabel(planning: PlanningAssignment): string {
        const employee = this._resolveEmployeeNames(planning).join(', ').trim();
        const groupe = (planning.groupeLabel || '').trim();
        const shift = (planning.shiftLabel || '').trim() || 'Shift inconnu';
        const subject = employee || groupe || 'Affectation inconnue';
        return `${subject} - ${shift}`;
    }

    private _splitCsvValues(value: string): string[] {
        return (value || '')
            .split(/[,;|]/)
            .map((item) => item.trim())
            .filter((item) => item.length > 0);
    }

    private _normalizeId(value: unknown): string {
        return String(value ?? '').trim().toLowerCase();
    }

    private _resolveGroupeNames(planning: PlanningAssignment): string[] {
        const groupeIds = (planning.groupeIds ?? []).map((id) => this._normalizeId(id));
        const namesFromIds = this.groupes
            .filter((groupe) => groupeIds.includes(this._normalizeId(groupe.groupeId)))
            .map((groupe) => (groupe.nom ?? '').trim())
            .filter((name) => name.length > 0);

        if (namesFromIds.length > 0) {
            return namesFromIds;
        }

        return this._splitCsvValues(planning.groupeLabel);
    }

    private _resolveShiftNames(planning: PlanningAssignment): string[] {
        const shiftIds = (planning.shiftIds ?? []).map((id) => this._normalizeId(id));
        const labelsFromIds = this.shifts
            .filter((shift) => shiftIds.includes(this._normalizeId(shift.shiftId)))
            .map((shift) => (shift.label ?? '').trim())
            .filter((label) => label.length > 0);

        if (labelsFromIds.length > 0) {
            return labelsFromIds;
        }

        return this._splitCsvValues(planning.shiftLabel);
    }

    private _resolveEmployeeNames(planning: PlanningAssignment): string[] {
        const directEmployeeIds = (planning.employeeIds ?? []).map((id) => this._normalizeId(id));
        const namesFromDirectEmployees = this.employees
            .filter((employee) => directEmployeeIds.includes(this._normalizeId(employee.employeeId)))
            .map((employee) => `${employee.prenom} ${employee.nom}`.trim())
            .filter((name) => name.length > 0);

        if (namesFromDirectEmployees.length > 0) {
            return Array.from(new Set(namesFromDirectEmployees));
        }

        const groupeIds = (planning.groupeIds ?? []).map((id) => this._normalizeId(id));
        const matchingGroupes = groupeIds.length > 0
            ? this.groupes.filter((groupe) => groupeIds.includes(this._normalizeId(groupe.groupeId)))
            : this.groupes.filter((groupe) => this._resolveGroupeNames(planning).includes((groupe.nom ?? '').trim()));

        const employeeNames = matchingGroupes.flatMap((groupe) =>
            (groupe.employeeIds ?? [])
                .map((employeeId) => this.employees.find((employee) => this._normalizeId(employee.employeeId) === this._normalizeId(employeeId)))
                .filter((employee) => !!employee)
                .map((employee) => `${employee.prenom} ${employee.nom}`.trim())
                .filter((name) => name.length > 0)
        );

        return Array.from(new Set(employeeNames));
    }

    private _resolveGroupeIdsByLabel(label: string): string[] {
        const names = this._splitCsvValues(label);
        if (!names.length) {
            return [];
        }

        return this.groupes
            .filter((groupe) => names.includes((groupe.nom ?? '').trim()))
            .map((groupe) => String(groupe.groupeId));
    }

    private _resolveShiftIdsByLabel(label: string): string[] {
        const labels = this._splitCsvValues(label);
        if (!labels.length) {
            return [];
        }

        return this.shifts
            .filter((shift) => labels.includes((shift.label ?? '').trim()))
            .map((shift) => String(shift.shiftId));
    }

    private _buildHoveredPlanningItems(day: CalendarDay): HoveredPlanningItem[] {
        const items: HoveredPlanningItem[] = [];

        day.plannings.forEach((planning) => {
            const groupeIds = (planning.groupeIds ?? []).map((id) => this._normalizeId(id));
            const employeeIds = (planning.employeeIds ?? []).map((id) => this._normalizeId(id));
            const resolvedGroupes = this.groupes.filter((g) => groupeIds.includes(this._normalizeId(g.groupeId)));
            const resolvedEmployees = this.employees.filter((employee) =>
                employeeIds.includes(this._normalizeId(employee.employeeId))
            );
            const resolvedShifts = this._resolveShifts(planning);
            const shiftText = resolvedShifts.length > 0
                ? resolvedShifts.map((s) => s.label).join(' / ')
                : 'Shift inconnu';

            const deviceIds = (planning.deviceIds ?? []).map((id) => this._normalizeId(id));
            const deviceName = this.devices
                .filter((d) => deviceIds.includes(this._normalizeId(d.deviceId)))
                .map((d) => d.deviceName)
                .join(', ');

            if (planning.assignmentMode === 'employee' || resolvedEmployees.length > 0) {
                const employeeEntries = resolvedEmployees.length > 0
                    ? resolvedEmployees.map((employee) => ({
                        name: `${employee.prenom} ${employee.nom}`.trim(),
                        deviceName,
                    }))
                    : this._resolveEmployeeNames(planning).map((name) => ({ name, deviceName }));

                items.push({
                    id: planning.id,
                    label: `${employeeEntries.map((employee) => employee.name).join(', ') || 'Employe inconnu'} - ${shiftText}`,
                    planning,
                    employeeEntries,
                });
                return;
            }

            if (resolvedGroupes.length === 0) {
                items.push({
                    id: planning.id,
                    label: this._formatPlanningLabel(planning),
                    planning,
                    employeeEntries: [],
                });
                return;
            }

            resolvedGroupes.forEach((groupe) => {
                const empEntries = (groupe.employeeIds ?? [])
                    .map((empId) => this.employees.find((e) => this._normalizeId(e.employeeId) === this._normalizeId(empId)))
                    .filter((e) => !!e)
                    .map((e) => ({ name: `${e.prenom} ${e.nom}`, deviceName }));

                items.push({
                    id: planning.id,
                    label: `${(groupe.nom ?? '').trim()} - ${shiftText}`,
                    planning,
                    employeeEntries: empEntries,
                });
            });
        });

        return items;
    }

    private _resolveShifts(planning: PlanningAssignment): Shift[] {
        const shiftIds = (planning.shiftIds ?? []).map((id) => this._normalizeId(id));
        return this.shifts.filter((s) => shiftIds.includes(this._normalizeId(s.shiftId)));
    }

    private _formatTime(time: string): string {
        if (!time) return '';
        // Handle "HH:mm:ss" or "HH:mm" formats
        const parts = time.split(':');
        return parts.length >= 2 ? `${parts[0]}:${parts[1]}` : time;
    }

    private _clearPopupHideTimer(): void {
        if (this._hidePopupTimer) {
            clearTimeout(this._hidePopupTimer);
            this._hidePopupTimer = null;
        }
    }

    getHolidayLabel(date: Date): string {
        if (!date || !this.holidays?.length) {
            return '';
        }

        const checkDate = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
        const holiday = this.holidays.find((item) => {
            const holidayDate = this._parseLocalDate(item.date);
            return new Date(holidayDate.getFullYear(), holidayDate.getMonth(), holidayDate.getDate()).getTime() === checkDate;
        });

        return (holiday?.label || '').trim();
    }

    private _updateAffectedPopupPosition(event: MouseEvent): void {
        const offset = 14;
        const popupWidth = Math.min(560, Math.max(320, window.innerWidth - 24));
        const popupHeight = Math.min(720, Math.max(280, window.innerHeight - 24));
        let x = event.clientX + offset;
        let y = event.clientY + offset;

        if (x + popupWidth > window.innerWidth - 8) {
            x = event.clientX - popupWidth - offset;
        }
        if (y + popupHeight > window.innerHeight - 8) {
            y = window.innerHeight - popupHeight - 8;
        }
        if (y < 8) {
            y = 8;
        }
        if (x < 8) {
            x = 8;
        }

        this.affectedPopupX = x;
        this.affectedPopupY = y;
    }

    ngOnDestroy(): void {
        document.removeEventListener('planningPreviousMonth', this._onPreviousYearHandler);
        document.removeEventListener('planningNextMonth', this._onNextYearHandler);
        document.removeEventListener('planningGoToToday', this._onGoToTodayHandler);
        this._clearPopupHideTimer();

        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
