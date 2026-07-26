import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { Groupe } from '../../../../core/groupe/groupe.model';
import { Employee } from '../../../../core/employee/employee.model';
import { Shift } from '../../../../core/shift/shift.model';
import { Device } from '../../../../core/device/device.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { PlanningService } from '../../../../core/planning/planning.service';
import { Planning } from '../../../../core/planning/planning.model';

@Component({
    selector: 'app-planning-dialog',
    standalone: true,
    imports: [
        CommonModule,
        TranslocoDirective,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatSelectModule,
        FormsModule,
        ReactiveFormsModule
    ],
    templateUrl: './planning-dialog.component.html',
    styleUrl: './planning-dialog.component.scss',
    encapsulation: ViewEncapsulation.None
})
export class PlanningDialogComponent implements OnInit {
    form: UntypedFormGroup;
    groupes: Groupe[] = [];
    employees: Employee[] = [];
    shifts: Shift[] = [];
    devices: Device[] = [];
    date: Date;
    selectedDates: Date[] = [];
    existingPlannings: any[] = [];
    assignMode: 'group' | 'employee' = 'group';

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: any,
        private _dialogRef: MatDialogRef<PlanningDialogComponent>,
        private _formBuilder: UntypedFormBuilder,
        private _planningService: PlanningService
    ) {
        this.selectedDates = (data?.selectedDates ?? []).map((item: any) =>
            item instanceof Date ? item : this._parseLocalDate(String(item))
        );
        if (this.selectedDates.length === 0 && data?.date) {
            this.selectedDates = [this._parseLocalDate(String(data.date))];
        }

        this.date = this.selectedDates[0] ?? new Date();
        this.groupes = data.groupes || [];
        this.employees = data.employees || [];
        this.shifts = data.shifts || [];
        this.devices = data.devices || [];
        this.existingPlannings = data.existingPlannings || [];
    }

    ngOnInit(): void {
        const isForceNew = !!this.data?.forceNew;
        const selectedPlanningId = this.data?.selectedPlanningId as string | undefined;
        const selectedPlanningSnapshot = this.data?.selectedPlanningSnapshot as any;
        const preselectedGroupeIds = this._toStringArray(this.data?.preselectedGroupeIds ?? []);
        const preselectedEmployeeIds = this._toStringArray(this.data?.preselectedEmployeeIds ?? []);
        const preselectedShiftIds = this._toStringArray(this.data?.preselectedShiftIds ?? []);
        const preselectedDeviceIds = this._toStringArray(this.data?.preselectedDeviceIds ?? []);
        const selectedPlanning =
            selectedPlanningId
                ? this.existingPlannings.find((p) => p?.id === selectedPlanningId) ?? null
                : null;
        const initialData = !isForceNew
            ? selectedPlanning ?? selectedPlanningSnapshot ?? (this.existingPlannings.length > 0 ? this.existingPlannings[0] : null)
            : null;

        const groupeIds =
            preselectedGroupeIds.length > 0
                ? preselectedGroupeIds
                : this._toStringArray(initialData?.groupeIds ?? []);
        const employeeIds =
            preselectedEmployeeIds.length > 0
                ? preselectedEmployeeIds
                : this._toStringArray(initialData?.employeeIds ?? []);
        const shiftIds =
            preselectedShiftIds.length > 0
                ? preselectedShiftIds
                : this._toStringArray(initialData?.shiftIds ?? []);
        const deviceIds =
            preselectedDeviceIds.length > 0
                ? preselectedDeviceIds
                : this._toStringArray(initialData?.deviceIds ?? []);

        this.form = this._formBuilder.group({
            planningId: [initialData ? initialData.id : 'new'],
            groupeIds: [groupeIds],
            employeeIds: [employeeIds],
            shiftIds: [shiftIds, Validators.required],
            deviceIds: [deviceIds]
        });

        if (employeeIds.length > 0 && groupeIds.length === 0) {
            this.assignMode = 'employee';
        }

        if (!isForceNew) {
            if (selectedPlanningId) {
                this._planningService.GetPlanningById(selectedPlanningId).subscribe((planning) => {
                    if (!planning) {
                        this._patchFormFromSnapshot(selectedPlanningId, selectedPlanningSnapshot);
                        return;
                    }
                    this._patchFormFromPlanning(planning);
                    this._patchMissingIdsFromSnapshot(selectedPlanningId, selectedPlanningSnapshot);
                });
            } else {
                this._patchFormFromSnapshot('', selectedPlanningSnapshot);
            }
        }
    }

    save(): void {
        if (this.form.invalid) return;

        const formValue = this.form.getRawValue();
        const groupeIds = this._getEffectiveGroupeIds();
        const dates = this.selectedDates.length > 0
            ? this.selectedDates.map((item) => this._toLocalIsoDate(item))
            : [this._toLocalIsoDate(this.date)];

        const planning = {
            ...formValue,
            groupeIds,
            date: dates[0] ?? this._toLocalIsoDate(this.date),
            dates
        };

        if (planning.planningId === 'new') {
            this._planningService.AddPlanning(planning).subscribe(() => {
                this._dialogRef.close(true);
            });
        } else {
            this._planningService.UpdatePlanning(planning).subscribe(() => {
                this._dialogRef.close(true);
            });
        }
    }

    delete(): void {
        const planningId = this.form.get('planningId')?.value;
        if (planningId && planningId !== 'new') {
            this._planningService.DeletePlanning({ planningId }).subscribe(() => {
                this._dialogRef.close(true);
            });
        }
    }

    close(): void {
        this._dialogRef.close(false);
    }

    private _toLocalIsoDate(date: Date): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    private _parseLocalDate(value: string): Date {
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

    private _toStringArray(value: unknown): string[] {
        if (!Array.isArray(value)) {
            return [];
        }
        return value
            .map((item) => String(item ?? '').trim())
            .filter((item) => item.length > 0);
    }

    private _patchFormFromPlanning(planning: Planning): void {
        this.form.patchValue({
            planningId: planning.planningId,
            groupeIds: this._mapIdsToAvailable(
                this._toStringArray(planning.groupeIds ?? []),
                this.groupes.map((x) => String(x.groupeId))
            ),
            employeeIds: this._mapIdsToAvailable(
                this._toStringArray(planning.employeeIds ?? []),
                this.employees.map((x) => String(x.employeeId))
            ),
            shiftIds: this._mapIdsToAvailable(
                this._toStringArray(planning.shiftIds ?? []),
                this.shifts.map((x) => String(x.shiftId))
            ),
            deviceIds: this._mapIdsToAvailable(
                this._toStringArray(planning.deviceIds ?? []),
                this.devices.map((x) => String(x.deviceId))
            ),
        });
    }

    private _patchMissingIdsFromSnapshot(selectedPlanningId: string, snapshot: any): void {
        const currentGroupeIds = this._toStringArray(this.form.get('groupeIds')?.value ?? []);
        const currentShiftIds = this._toStringArray(this.form.get('shiftIds')?.value ?? []);

        if (currentGroupeIds.length > 0 && currentShiftIds.length > 0) {
            return;
        }

        this._patchFormFromSnapshot(selectedPlanningId, snapshot, true);
    }

    private _patchFormFromSnapshot(selectedPlanningId: string, snapshot: any, mergeOnly = false): void {
        if (!snapshot) {
            return;
        }

        const groupeIdsFromSnapshot = this._toStringArray(snapshot.groupeIds ?? []);
        const employeeIdsFromSnapshot = this._toStringArray(snapshot.employeeIds ?? []);
        const shiftIdsFromSnapshot = this._toStringArray(snapshot.shiftIds ?? []);
        const deviceIdsFromSnapshot = this._toStringArray(snapshot.deviceIds ?? []);

        const groupeIds = groupeIdsFromSnapshot.length > 0
            ? groupeIdsFromSnapshot
            : this._resolveGroupeIdsByLabel(snapshot.groupeLabel);
        const shiftIds = shiftIdsFromSnapshot.length > 0
            ? shiftIdsFromSnapshot
            : this._resolveShiftIdsByLabel(snapshot.shiftLabel);

        const nextGroupeIds = mergeOnly
            ? (this._toStringArray(this.form.get('groupeIds')?.value ?? []).length > 0
                ? this._toStringArray(this.form.get('groupeIds')?.value ?? [])
                : groupeIds)
            : groupeIds;
        const nextEmployeeIds = mergeOnly
            ? (this._toStringArray(this.form.get('employeeIds')?.value ?? []).length > 0
                ? this._toStringArray(this.form.get('employeeIds')?.value ?? [])
                : employeeIdsFromSnapshot)
            : employeeIdsFromSnapshot;
        const nextShiftIds = mergeOnly
            ? (this._toStringArray(this.form.get('shiftIds')?.value ?? []).length > 0
                ? this._toStringArray(this.form.get('shiftIds')?.value ?? [])
                : shiftIds)
            : shiftIds;
        const nextDeviceIds = mergeOnly
            ? (this._toStringArray(this.form.get('deviceIds')?.value ?? []).length > 0
                ? this._toStringArray(this.form.get('deviceIds')?.value ?? [])
                : deviceIdsFromSnapshot)
            : deviceIdsFromSnapshot;

        this.form.patchValue({
            planningId: selectedPlanningId || snapshot.id || 'new',
            groupeIds: this._mapIdsToAvailable(
                nextGroupeIds,
                this.groupes.map((x) => String(x.groupeId))
            ),
            employeeIds: this._mapIdsToAvailable(
                nextEmployeeIds,
                this.employees.map((x) => String(x.employeeId))
            ),
            shiftIds: this._mapIdsToAvailable(
                nextShiftIds,
                this.shifts.map((x) => String(x.shiftId))
            ),
            deviceIds: this._mapIdsToAvailable(
                nextDeviceIds,
                this.devices.map((x) => String(x.deviceId))
            ),
        });
    }

    private _resolveGroupeIdsByLabel(label: string): string[] {
        const names = this._splitCsv(label);
        if (!names.length) {
            return [];
        }

        return this.groupes
            .filter((groupe) => names.includes((groupe.nom ?? '').trim()))
            .map((groupe) => String(groupe.groupeId));
    }

    private _resolveShiftIdsByLabel(label: string): string[] {
        const labels = this._splitCsv(label);
        if (!labels.length) {
            return [];
        }

        return this.shifts
            .filter((shift) => labels.includes((shift.label ?? '').trim()))
            .map((shift) => String(shift.shiftId));
    }

    private _splitCsv(value: string): string[] {
        return (value || '')
            .split(',')
            .map((item) => item.trim())
            .filter((item) => item.length > 0);
    }

    private _getEffectiveGroupeIds(): string[] {
        const selectedGroupeIds = this._toStringArray(this.form.get('groupeIds')?.value ?? []);
        const selectedEmployeeIds = this._toStringArray(this.form.get('employeeIds')?.value ?? []);
        const derivedGroupeIds = this.groupes
            .filter((groupe) =>
                (groupe.employeeIds ?? []).some((employeeId) =>
                    selectedEmployeeIds.some((selectedId) => this._normalizeId(selectedId) === this._normalizeId(employeeId))
                )
            )
            .map((groupe) => String(groupe.groupeId));

        return Array.from(new Set([...selectedGroupeIds, ...derivedGroupeIds]));
    }

    get hasSelection(): boolean {
        const groupeIds = this._toStringArray(this.form.get('groupeIds')?.value ?? []);
        const employeeIds = this._toStringArray(this.form.get('employeeIds')?.value ?? []);
        return groupeIds.length > 0 || employeeIds.length > 0;
    }

    isGroupeSelected(groupeId: string): boolean {
        const ids: string[] = this.form.get('groupeIds')?.value ?? [];
        return ids.some(id => this._normalizeId(id) === this._normalizeId(groupeId));
    }

    toggleGroupe(groupeId: string): void {
        const current: string[] = [...(this.form.get('groupeIds')?.value ?? [])];
        const strId = String(groupeId);
        const idx = current.findIndex(id => this._normalizeId(id) === this._normalizeId(strId));
        if (idx >= 0) {
            current.splice(idx, 1);
        } else {
            current.push(strId);
        }
        this.form.patchValue({ groupeIds: current });
    }

    isEmployeeSelected(employeeId: string): boolean {
        const ids: string[] = this.form.get('employeeIds')?.value ?? [];
        return ids.some(id => this._normalizeId(id) === this._normalizeId(employeeId));
    }

    toggleEmployee(employeeId: string): void {
        const current: string[] = [...(this.form.get('employeeIds')?.value ?? [])];
        const strId = String(employeeId);
        const idx = current.findIndex(id => this._normalizeId(id) === this._normalizeId(strId));
        if (idx >= 0) {
            current.splice(idx, 1);
        } else {
            current.push(strId);
        }
        this.form.patchValue({ employeeIds: current });
    }

    compareIds = (a: unknown, b: unknown): boolean => {
        return this._normalizeId(a) === this._normalizeId(b);
    };

    private _mapIdsToAvailable(ids: string[], availableIds: string[]): string[] {
        const availableMap = new Map<string, string>();
        availableIds.forEach((id) => {
            availableMap.set(this._normalizeId(id), String(id));
        });

        const resolved = ids
            .map((id) => availableMap.get(this._normalizeId(id)) ?? String(id))
            .filter((id) => id.length > 0);

        return Array.from(new Set(resolved));
    }

    private _normalizeId(value: unknown): string {
        if (value === null || value === undefined) {
            return '';
        }
        if (typeof value === 'object') {
            const obj = value as Record<string, unknown>;
            const raw = obj['id'] ?? obj['groupeId'] ?? obj['employeeId'] ?? obj['shiftId'] ?? obj['deviceId'] ?? '';
            return String(raw).trim().toLowerCase();
        }
        return String(value).trim().toLowerCase();
    }
}
