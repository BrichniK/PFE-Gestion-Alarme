import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { AlerteService } from '../../../../core/alerte/alerte.service';
import { EmployeePlanning, GroupeWithEmployees } from '../../../../core/alerte/alerte.model';
import { TranslocoDirective } from '@ngneat/transloco';

export interface TraiterDialogData {
    alerteDate: string;
    dispositifId: string;
}

@Component({
    selector: 'app-traiter-dialog',
    standalone: true,
    imports: [
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatRadioModule,
        MatProgressSpinnerModule,
        FormsModule,
        TranslocoDirective,
    ],
    templateUrl: './traiter-dialog.component.html',
    styleUrl: './traiter-dialog.component.scss',
})
export class TraiterDialogComponent implements OnInit {
    groupes: GroupeWithEmployees[] = [];
    selectedEmployeeId: string | null = null;
    isLoading = true;

    constructor(
        private _dialogRef: MatDialogRef<TraiterDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: TraiterDialogData,
        private _alerteService: AlerteService
    ) { }

    ngOnInit(): void {
        this._alerteService
            .GetEmployeesByPlanning(this.data.alerteDate, this.data.dispositifId)
            .subscribe((groupes) => {
                this.groupes = groupes;
                this._autoSelectSingleEmployee();
                this.isLoading = false;
            });
    }

    confirm(): void {
        if (this.selectedEmployeeId) {
            this._dialogRef.close({ employeeId: this.selectedEmployeeId });
        }
    }

    cancel(): void {
        this._dialogRef.close(null);
    }

    private _buildUniqueEmployees(groupes: GroupeWithEmployees[]): EmployeePlanning[] {
        const employeeMap = new Map<string, EmployeePlanning>();

        groupes.forEach((groupe) => {
            (groupe.employees ?? []).forEach((employee) => {
                employeeMap.set(employee.employeeId, employee);
            });
        });

        return Array.from(employeeMap.values());
    }

    private _autoSelectSingleEmployee(): void {
        const candidates = this._buildUniqueEmployees(this.groupes);

        if (candidates.length === 1) {
            this.selectedEmployeeId = candidates[0].employeeId;
            return;
        }

        if (this.selectedEmployeeId && candidates.some((employee) => employee.employeeId === this.selectedEmployeeId)) {
            return;
        }

        this.selectedEmployeeId = null;
    }
}
