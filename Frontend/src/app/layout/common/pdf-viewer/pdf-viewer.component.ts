import {
    Component,
    ElementRef,
    inject,
    OnDestroy,
    OnInit,
    Renderer2,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { fuseAnimations } from '../../../../@fuse/animations';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import * as pdfjsLib from 'pdfjs-dist';
import { PDFDocumentProxy, RenderParameters, TypedArray } from 'pdfjs-dist/types/src/display/api';
import { catchError, Observable, Subject, takeUntil } from 'rxjs';
import { ReportViewerService } from '../../../core/report-viewer/report-viewer.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { NgClass } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FuseLoadingService } from '../../../../@fuse/services/loading';
import { Title } from '@angular/platform-browser';
import { fromPromise } from 'rxjs/internal/observable/innerFrom';
import { PdfViewerConfig } from './pdf-viewer.model';
// import { TextLayer } from 'pdfjs-dist';
@Component({
    selector: 'pdf-viewer',
    standalone: true,
    imports: [MatProgressSpinnerModule,
        MatIconModule,
        MatButtonModule,
        MatDialogModule,
        MatMenuModule,
        MatButtonToggleModule,
        NgClass,
        MatTooltipModule,
    ],
    templateUrl: './pdf-viewer.component.html',
    styleUrl: './pdf-viewer.component.scss',
    encapsulation: ViewEncapsulation.None,
    exportAs: 'pdfViewer',
    animations: fuseAnimations,
})
export class PdfViewerComponent implements OnInit, OnDestroy {
    config: PdfViewerConfig = inject(MAT_DIALOG_DATA);
    private _renderer:Renderer2 = inject(Renderer2);
    private _fuseLoadingService = inject(FuseLoadingService);
    private _reportViewerService = inject(ReportViewerService);
    private _title = inject(Title);

    @ViewChild('pdfContainer', { static: true }) pdfContainer!: ElementRef<HTMLDivElement>;

    title='Aperçu du rapport';
    pdfDocument: PDFDocumentProxy = null;
    totalPages = 0;
    scale = 1.2;
    //searchText = '';
    currentPage = 1;
    flashMessage: 'error' | null = null;
    isDocumentLoading: boolean = true;
    isLandscape: boolean = false;
    private _oldTitle : string;
    private _canvasElements : HTMLCanvasElement[] = [];
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    ngOnInit(): void {
        this._fuseLoadingService.setAutoMode(false);
        this.getPDFData()
            .subscribe((data) => {
                this.initData(data);
            });

        this._oldTitle = this._title.getTitle();
    }

    getPDFData():Observable<ArrayBuffer>{
        this.pdfContainer.nativeElement.innerHTML = '';
        this._canvasElements = [];
        this.pdfDocument = null;
        this.scale = 1.2;

        return this._reportViewerService.GetReportData(this.config.url, {
            params: {
                ...this.config.params, islandscape: this.isLandscape
            }
        })
            .pipe(
                takeUntil(this._unsubscribeAll))
    }

    private initData(data:ArrayBuffer){
        if (!data || data?.byteLength === 0) {
            this.isDocumentLoading = false;
            return;
        }

        fromPromise(this.loadPdf(data))
            .pipe(
                catchError(_=>this.flashMessage='error'),
                takeUntil(this._unsubscribeAll))
            .subscribe();

        this.isDocumentLoading = false;
    }

    private async loadPdf(pdfData:string | TypedArray | ArrayBuffer | number[]): Promise<void> {
        try {
            pdfjsLib.GlobalWorkerOptions.workerSrc = './pdf-dist/pdf.worker.min.mjs';

            this.pdfDocument = await pdfjsLib.getDocument({ data: pdfData }).promise;
            this.totalPages = this.pdfDocument.numPages;

            const metadata = await this.pdfDocument.getMetadata();
            this.title = (metadata?.info as any)?.Title || 'Aperçu du rapport';
            this._title.setTitle(this.title);

            this._canvasElements = [];
            await this.renderAllPages();
        }catch {
            this.flashMessage='error';
            this.isDocumentLoading = false;
            this.pdfContainer.nativeElement.innerHTML = '';
        }
    }

    private async renderAllPages(): Promise<void> {
        this.pdfContainer.nativeElement.innerHTML = '';

        for (let pageNumber = 1; pageNumber <= this.totalPages; pageNumber++) {
            await this.renderPage(pageNumber);
        }
    }

    private async renderPage(pageNumber: number): Promise<void> {

        const page = await this.pdfDocument.getPage(pageNumber);
        const viewport = page.getViewport({ scale: this.scale*2 });

        const canvas:HTMLCanvasElement = this._renderer.createElement('canvas');
        const context = canvas.getContext('2d')!;
        canvas.width = viewport.width;
        canvas.style.width = ((canvas.width/2) * this.scale)+"px";
        canvas.height = viewport.height;
        canvas.style.height = ((canvas.height/2) * this.scale)+"px";

        const renderContext:RenderParameters = {
            canvasContext: context,
            viewport,
        };

        await page.render(renderContext).promise;

        this._canvasElements.push(canvas);
        this.pdfContainer.nativeElement.appendChild(canvas);
    }

    updateZoom(){
        for (let canvas of this._canvasElements){
            canvas.style.width = ((canvas.width/2) * this.scale) + 'px';
            canvas.style.height = ((canvas.height/2) * this.scale) + 'px';
        }
    }

    zoomIn(): void {
        this.scale = Math.min(3.0, this.scale + 0.2);
        this.updateZoom()
    }

    zoomOut(): void {
        this.scale = Math.max(0.2, this.scale - 0.2);
        this.updateZoom()
    }

    changeOrientation(){
        this.isLandscape = !this.isLandscape;
        this.isDocumentLoading=true;

        this.getPDFData()
            .subscribe( data => {
                this.initData(data);
            });
    }

    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.pdfContainer.nativeElement.scrollTo({
                top: this.pdfContainer.nativeElement.children[page - 1].scrollTop,
                behavior: 'smooth',
            });
        }
    }

    async downloadPdf() {
        const data = await this.pdfDocument.getData()
        const blob = new Blob([data], { type: 'application/pdf' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = this.title+'.pdf';
        a.click();
        URL.revokeObjectURL(url);
    }

    async downloadDocx() {
        this._reportViewerService.GetReportData(this.config.url, {
            params: {
                ...this.config.params, islandscape: this.isLandscape, format: 2
            }
        })
            .subscribe(async (data) => {
                const blob = new Blob([data], { type: 'application/docx' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = this.title+'.docx';
                a.click();
                URL.revokeObjectURL(url);
            })

    }

    async downloadExcel() {
        this._reportViewerService.GetReportData(this.config.url, {
            params: {
                ...this.config.params, islandscape: this.isLandscape, format: 1
            }
        })
            .subscribe(async (data) => {
                const blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = this.title+'.xlsx';
                a.click();
                URL.revokeObjectURL(url);
            })

    }

    async printPdf() {
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';
        const data = await this.pdfDocument.getData();
        const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }));
        iframe.src = url;
        document.body.appendChild(iframe);
        iframe.contentWindow?.focus();
        iframe.contentWindow?.print();
        URL.revokeObjectURL(url);
    }

    searchInPdf(): void {
        Array.from(this.pdfContainer.nativeElement.children).forEach((canvas:HTMLCanvasElement) => {
            const context = (canvas as HTMLCanvasElement).getContext('2d')!;
            const text = context.getImageData(0, 0, canvas.width, canvas.height);
            // Custom logic for text search goes here
        });
    }

    ngOnDestroy(): void {
        this._fuseLoadingService.setAutoMode(true);
        this._title.setTitle(this._oldTitle);
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }
}
