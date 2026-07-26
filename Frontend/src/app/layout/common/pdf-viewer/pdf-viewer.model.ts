import { HttpParams } from '@angular/common/http';

export interface PdfViewerConfig {
    title?: string;
    url:string;
    isExcelSupported?:boolean;
    params?:HttpParams | {
        [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>;
    };
}
