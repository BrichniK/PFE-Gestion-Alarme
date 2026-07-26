import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './api-response';
import { catchError, Observable, of, tap } from 'rxjs';
import { SettingConfigService } from '../config/setting-config.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarComponent } from '../../../@fuse/components/snackbar/snackbar.component';
import { SnackBarData } from '../../../@fuse/components/snackbar/snackbar.model';

@Injectable({
    providedIn: 'root',
})
export class ApiService {
    private _settingService = inject(SettingConfigService);
    private _snackBar = inject(MatSnackBar);
    private _basePath = environment.BaseApi;

    constructor(public _httpClient: HttpClient) {}

    private showToast(data: SnackBarData): void {
        this._snackBar.openFromComponent(SnackbarComponent, {
            duration: 4500,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            data,
        });
    }

    private toUserErrorMessage(rawMessage?: string): string {
        const message = (rawMessage ?? '').trim();
        if (!message) {
            return "une erreur s'est produite, veuillez contacter votre administrateur ou reessayer";
        }

        if (message.includes('FK_Maintenance_Employee_EmployeeId')) {
            return "suppression impossible: cet employe est lie a des maintenances. Supprimez ou reaffectez d'abord ces maintenances.";
        }

        if (message.includes('REFERENCE constraint')) {
            return "suppression impossible: cet element est encore utilise dans d'autres enregistrements.";
        }

        return message.split('\n')[0];
    }

    Post<TData>(url: string, data: any): Observable<ApiResponse<TData>> {
        return this._httpClient.post<ApiResponse<TData>>(this._settingService.baseApi + url, data).pipe(
            tap((response) => {
                if (response?.success) {
                    this.showToast({ message: 'Enregistre avec succes.', type: 'success' });
                }
            }),
            catchError((error) => {
                this.showToast({ message: `Erreur: ${this.toUserErrorMessage(error?.error?.message)}`, type: 'error' });
                return this.handleError<ApiResponse<TData>>(error);
            })
        );
    }

    Post2<TData>(url: string, data: any): Observable<ApiResponse<TData>> {
        return this._httpClient.post<ApiResponse<TData>>(this._settingService.baseApi + url, data).pipe(
            tap((response) => {
                if (response?.success) {
                }
            }),
            catchError((error) => {
                return this.handleError<ApiResponse<TData>>(error);
            })
        );
    }

    Get<TData>(
        url: string,
        options?: {
            params?: HttpParams | { [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean> };
        }
    ): Observable<ApiResponse<TData>> {
        return this._httpClient
            .get<ApiResponse<TData>>(this._settingService.baseApi + url, options)
            .pipe(catchError(this.handleError<ApiResponse<TData>>));
    }

    GetFile(
        url: string,
        options?: {
            params?: HttpParams | { [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean> };
        }
    ): Observable<ArrayBuffer> {
        return this._httpClient
            .get(this._settingService.baseApi + url, {
                ...options,
                responseType: 'arraybuffer',
            })
            .pipe(
                catchError((error) => {
                    this.showToast({ message: `Erreur: ${this.toUserErrorMessage(error?.error?.message)}`, type: 'error' });
                    return of(null);
                })
            );
    }

    GetImage(url: string): Observable<any> {
        return this._httpClient.get(this._settingService.baseApi + url, { responseType: 'blob' }).pipe(catchError(this.handleError));
    }

    Patch<TData>(url: string, data: any): Observable<ApiResponse<TData>> {
        return this._httpClient.patch<ApiResponse<TData>>(this._settingService.baseApi + url, data).pipe(
            tap((response) => {
                if (response?.success) {
                    this.showToast({ message: 'Enregistre avec succes.', type: 'success' });
                }
            }),
            catchError((error) => {
                this.showToast({ message: `Erreur: ${this.toUserErrorMessage(error?.error?.message)}`, type: 'error' });
                return this.handleError<ApiResponse<TData>>(error);
            })
        );
    }

    Delete<TData>(
        url: string,
        options?: {
            params?: HttpParams | { [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean> };
        }
    ): Observable<ApiResponse<TData>> {
        return this._httpClient.delete<ApiResponse<TData>>(this._settingService.baseApi + url, options).pipe(
            tap((response) => {
                if (response?.success) {
                    this.showToast({ message: 'Supprime avec succes.', type: 'success' });
                }
            }),
            catchError((error) => {
                this.showToast({ message: `Erreur: ${this.toUserErrorMessage(error?.error?.message)}`, type: 'error' });
                return this.handleError<ApiResponse<TData>>(error);
            })
        );
    }

    SilentPost<TData>(url: string, data: any): Observable<ApiResponse<TData>> {
        return this._httpClient
            .post<ApiResponse<TData>>(this._settingService.baseApi + url, data)
            .pipe(catchError(this.handleError<ApiResponse<TData>>));
    }

    GetBlob(url: string): Observable<Blob> {
        return this._httpClient.get(this._basePath + url, {
            responseType: 'blob',
        });
    }

    GetPdf(url: string): Observable<any> {
        return this._httpClient.get(this._basePath + url, { responseType: 'blob' }).pipe(catchError(this.handleError));
    }

    private handleError<T>(error: HttpErrorResponse) {
        console.error(error);
        return of(error.error as T);
    }
}
