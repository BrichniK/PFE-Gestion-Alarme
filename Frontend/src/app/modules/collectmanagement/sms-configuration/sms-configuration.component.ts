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
    Validators,
} from '@angular/forms';
import { Subject, finalize, takeUntil } from 'rxjs';
import { SMSConfigurationService } from '../../../core/sms-configuration/sms-configuration.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoDirective } from '@ngneat/transloco';
import { fuseAnimations } from '../../../../@fuse/animations';

@Component({
    selector: 'app-sms-configuration',
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
    templateUrl: './sms-configuration.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class SMSConfigurationComponent implements OnInit, OnDestroy {
    configForm: UntypedFormGroup;
    isLoading = true;
    isSaving = false;
    flashMessage: 'success' | 'error' | null = null;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _smsConfigService: SMSConfigurationService,
        private _formBuilder: UntypedFormBuilder,
        private _changeDetectorRef: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this.configForm = this._formBuilder.group({
            apiUrl: ['', [Validators.required]],
            isActive: [false],
            nombreAlerte: [1, [Validators.required, Validators.min(1)]],
            delai: [0, [Validators.required, Validators.min(0)]],
            smsOnAlerte: [true],
            smsOnBadgeT3: [true],
            smsOnBadgeT4: [true],
            smsOnBadgeT5: [true],
            smsOnTraitement: [true],
        });

        this._smsConfigService
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
                    apiUrl: config?.apiUrl ?? '',
                    isActive: config?.isActive ?? false,
                    nombreAlerte: config?.nombreAlerte ?? 1,
                    delai: config?.delai ?? 0,
                    smsOnAlerte: config?.smsOnAlerte ?? true,
                    smsOnBadgeT3: config?.smsOnBadgeT3 ?? true,
                    smsOnBadgeT4: config?.smsOnBadgeT4 ?? true,
                    smsOnBadgeT5: config?.smsOnBadgeT5 ?? true,
                    smsOnTraitement: config?.smsOnTraitement ?? true,
                });
                this._changeDetectorRef.markForCheck();
            });
    }

    save(): void {
        if (this.configForm.invalid) {
            return;
        }

        this.isSaving = true;
        this._changeDetectorRef.markForCheck();

        const { apiUrl, isActive, nombreAlerte, delai, smsOnAlerte, smsOnBadgeT3, smsOnBadgeT4, smsOnBadgeT5, smsOnTraitement } = this.configForm.getRawValue();

        this._smsConfigService
            .UpdateConfiguration({ apiUrl, isActive, nombreAlerte, delai, smsOnAlerte, smsOnBadgeT3, smsOnBadgeT4, smsOnBadgeT5, smsOnTraitement })
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
