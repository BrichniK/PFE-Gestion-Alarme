import { inject, Injectable } from '@angular/core';
import { ApiService } from '../common/api.service';
import { map, Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class ReportViewerService {
    private _apiService = inject(ApiService);

    /** By AJ
     * Get report data content from "Byte[] .Net => Uint8Array"
     * @param url file url required
     * @param options used to pass params
     * @returns an Observable of Uint8Array of the file
     */
    GetReportData(url: string, options?: { params?:HttpParams | {
            [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>;
        };
    }) : Observable<ArrayBuffer> {
        return this._apiService.GetFile(url, options);
    }
}
