import { Component, inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarAction, MatSnackBarRef } from '@angular/material/snack-bar';
import { SnackBarData } from './snackbar.model';
import { NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'app-snackbar',
    standalone: true,
    imports: [
        NgClass,
        MatIconModule,
        MatButtonModule,
        MatSnackBarAction,
    ],
    templateUrl: './snackbar.component.html',
})
export class SnackbarComponent {
    data: SnackBarData = inject(MAT_SNACK_BAR_DATA);
    snackBarRef = inject(MatSnackBarRef);
}
