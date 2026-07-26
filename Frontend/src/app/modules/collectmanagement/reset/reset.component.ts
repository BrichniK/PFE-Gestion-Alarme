import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { Subject, takeUntil, finalize } from 'rxjs';
import { ResetService } from '../../../core/reset/reset.service';
import { DeviceService } from '../../../core/device/device.service';
import { Device } from '../../../core/device/device.model';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoDirective } from '@ngneat/transloco';
import { fuseAnimations } from '../../../../@fuse/animations';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-reset',
    standalone: true,
    imports: [
        CommonModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        MatIconModule,
        MatProgressSpinnerModule,
        TranslocoDirective,
    ],
    templateUrl: './reset.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class ResetComponent implements OnInit, OnDestroy {
    devices: Device[] = [];
    selectedDeviceId: string | null = null;
    isLoading = true;
    isResetting = false;
    flashMessage: 'success' | 'error' | null = null;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _resetService: ResetService,
        private _deviceService: DeviceService,
        private _changeDetectorRef: ChangeDetectorRef
    ) {}

    ngOnInit(): void {
        this._deviceService
            .GetDevice(1, 1000)
            .pipe(
                takeUntil(this._unsubscribeAll),
                finalize(() => {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe((result) => {
                this.devices = result?.devices ?? [];
                this._changeDetectorRef.markForCheck();
            });
    }

    resetDevice(): void {
        if (!this.selectedDeviceId) {
            return;
        }

        this.isResetting = true;
        this.flashMessage = null;
        this._changeDetectorRef.markForCheck();

        this._resetService
            .resetDevice(this.selectedDeviceId)
            .pipe(
                takeUntil(this._unsubscribeAll),
                finalize(() => {
                    this.isResetting = false;
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
                    }, 5000);
                },
                error: () => {
                    this.flashMessage = 'error';
                    this._changeDetectorRef.markForCheck();
                    setTimeout(() => {
                        this.flashMessage = null;
                        this._changeDetectorRef.markForCheck();
                    }, 5000);
                },
            });
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }
}
