import { inject, Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { PdfViewerComponent } from './pdf-viewer.component';
import { ApiService } from '../../../core/common/api.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { PdfViewerConfig } from './pdf-viewer.model';

@Injectable({
    providedIn: 'root'
})
export class PdfViewerService {
    private _matDialog: MatDialog = inject(MatDialog);

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------
    open(
        config: PdfViewerConfig
    ): MatDialogRef<PdfViewerComponent> {
        // Open the dialog
        return this._matDialog.open(PdfViewerComponent, {
            autoFocus: false,
            disableClose: false,
            data: config,
            panelClass: 'pdf-viewer-panel',
            width: '100%',
            height: '100%',
            maxWidth: '100%',
            maxHeight: '100%',
        });
    }
}
