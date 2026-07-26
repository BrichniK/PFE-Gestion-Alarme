import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormGroup,
} from '@angular/forms';
import { Subject, finalize, takeUntil } from 'rxjs';
import { ConfigurationGeneraleService } from '../../../core/configuration-generale/configuration-generale.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoDirective } from '@ngneat/transloco';
import { fuseAnimations } from '../../../../@fuse/animations';

@Component({
    selector: 'app-configuration-generale',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatSlideToggleModule,
        MatProgressSpinnerModule,
        TranslocoDirective,
    ],
    templateUrl: './configuration-generale.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class ConfigurationGeneraleComponent implements OnInit, OnDestroy {
    configForm: UntypedFormGroup;
    isLoading = true;
    isSaving = false;
    flashMessage: 'success' | 'error' | null = null;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _configService: ConfigurationGeneraleService,
        private _formBuilder: UntypedFormBuilder,
        private _changeDetectorRef: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this.configForm = this._formBuilder.group({
            ecraserEmployeMaintenance: [false],
            accepterSeulementEmployesPlanifies: [false],
            diagnostiqueObligatoire: [true],
            monitoringPourcentageSurSommeDurees: [true],
            coefficientGaugeD1: [1],
            coefficientGaugeD2: [1],
            coefficientGaugeD3: [1],
            coefficientGaugeD4: [1],
        });

        this._configService
            .GetConfiguration()
            .pipe(
                takeUntil(this._unsubscribeAll),
                finalize(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe((config) => {
                this.configForm.patchValue({
                    ecraserEmployeMaintenance:
                        config?.ecraserEmployeMaintenance ?? false,
                    accepterSeulementEmployesPlanifies:
                        config?.accepterSeulementEmployesPlanifies ?? false,
                    diagnostiqueObligatoire:
                        config?.diagnostiqueObligatoire ?? true,
                    monitoringPourcentageSurSommeDurees:
                        config?.monitoringPourcentageSurSommeDurees ?? true,
                    coefficientGaugeD1: config?.coefficientGaugeD1 ?? 1,
                    coefficientGaugeD2: config?.coefficientGaugeD2 ?? 1,
                    coefficientGaugeD3: config?.coefficientGaugeD3 ?? 1,
                    coefficientGaugeD4: config?.coefficientGaugeD4 ?? 1,
                });
                this._changeDetectorRef.markForCheck();
            });
    }

    save(): void {
        this.isSaving = true;
        this._changeDetectorRef.markForCheck();

        const {
            ecraserEmployeMaintenance,
            accepterSeulementEmployesPlanifies,
            diagnostiqueObligatoire,
            monitoringPourcentageSurSommeDurees,
            coefficientGaugeD1,
            coefficientGaugeD2,
            coefficientGaugeD3,
            coefficientGaugeD4,
        } = this.configForm.getRawValue();

        this._configService
            .UpdateConfiguration({
                ecraserEmployeMaintenance,
                accepterSeulementEmployesPlanifies,
                diagnostiqueObligatoire,
                monitoringPourcentageSurSommeDurees,
                coefficientGaugeD1: Number(coefficientGaugeD1) || 1,
                coefficientGaugeD2: Number(coefficientGaugeD2) || 1,
                coefficientGaugeD3: Number(coefficientGaugeD3) || 1,
                coefficientGaugeD4: Number(coefficientGaugeD4) || 1,
            })
            .pipe(
                takeUntil(this._unsubscribeAll),
                finalize(() => {
                    this.isSaving = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe({
                next: () => {
                    this.flashMessage = 'success';
                    this._changeDetectorRef.markForCheck();
                    setTimeout(() => {
                        this.flashMessage = null;
                        this._changeDetectorRef.markForCheck();
                    }, 3000);
                },
                error: () => {
                    this.flashMessage = 'error';
                    this._changeDetectorRef.markForCheck();
                    setTimeout(() => {
                        this.flashMessage = null;
                        this._changeDetectorRef.markForCheck();
                    }, 3000);
                },
            });
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }
}
