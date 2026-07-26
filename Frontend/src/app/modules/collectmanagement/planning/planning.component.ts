import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { fuseAnimations } from '../../../../@fuse/animations';
import { ActivatedRoute, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { RoleNavigation } from '../../../core/role-utilisateur/role-utilisateur.model';
import { TranslocoDirective } from '@ngneat/transloco';
import { PlanningService } from '../../../core/planning/planning.service';

@Component({
    selector: 'app-planning',
    standalone: true,
    imports: [
        CommonModule,
        MatIconModule,
        MatButtonModule,
        RouterOutlet,
        RouterLink,
        RouterLinkActive,
        TranslocoDirective,
    ],
    templateUrl: './planning.component.html',
    styleUrl: './planning.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class PlanningComponent implements OnInit, OnDestroy {
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roleNavigation: RoleNavigation;
    planningsLength: number = 0;
    currentMonthYear: string = '';
    private _currentDate: Date = new Date();
    private readonly _onPreviousMonthHandler = () => this._onPreviousMonth();
    private readonly _onNextMonthHandler = () => this._onNextMonth();
    private readonly _onGoToTodayHandler = () => this._onGoToToday();

    monthNames: string[] = [
        'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin',
        'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'
    ];

    constructor(
        private _activatedRoute: ActivatedRoute,
        private _changeDetectorRef: ChangeDetectorRef,
        private _planningService: PlanningService,
    ) { }

    ngOnInit(): void {
        this.updateMonthYearDisplay();

        // Listen for calendar navigation events from child
        document.addEventListener('planningPreviousMonth', this._onPreviousMonthHandler);
        document.addEventListener('planningNextMonth', this._onNextMonthHandler);
        document.addEventListener('planningGoToToday', this._onGoToTodayHandler);

        // Get resolver data
        this._activatedRoute.data
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data) => {
                if (data?.navigation) {
                    this.roleNavigation = data.navigation;
                }
                if (data?.plannings) {
                    this.planningsLength = data.plannings.length || 0;
                }
                this._changeDetectorRef.markForCheck();
            });

        this._planningService.planningsLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length) => {
                this.planningsLength = length;
                this._changeDetectorRef.markForCheck();
            });
    }

    private updateMonthYearDisplay(): void {
        this.currentMonthYear = `${this._currentDate.getFullYear()}`;
    }

    private _onPreviousMonth(): void {
        this._currentDate.setFullYear(this._currentDate.getFullYear() - 1);
        this.updateMonthYearDisplay();
        this._changeDetectorRef.markForCheck();
    }

    private _onNextMonth(): void {
        this._currentDate.setFullYear(this._currentDate.getFullYear() + 1);
        this.updateMonthYearDisplay();
        this._changeDetectorRef.markForCheck();
    }

    private _onGoToToday(): void {
        this._currentDate = new Date();
        this.updateMonthYearDisplay();
        this._changeDetectorRef.markForCheck();
    }

    dispatchPreviousMonth(): void {
        document.dispatchEvent(new CustomEvent('planningPreviousMonth'));
    }

    dispatchNextMonth(): void {
        document.dispatchEvent(new CustomEvent('planningNextMonth'));
    }

    dispatchGoToToday(): void {
        document.dispatchEvent(new CustomEvent('planningGoToToday'));
    }

    ngOnDestroy(): void {
        document.removeEventListener('planningPreviousMonth', this._onPreviousMonthHandler);
        document.removeEventListener('planningNextMonth', this._onNextMonthHandler);
        document.removeEventListener('planningGoToToday', this._onGoToTodayHandler);

        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }
}
