import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, tap, map, catchError } from 'rxjs';
import { PagedSMS, SMS } from './sms.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root'
})
export class SMSService {

    private _smss: BehaviorSubject<SMS[] | null> = new BehaviorSubject([]);
    private _sms: BehaviorSubject<SMS | null> = new BehaviorSubject(null);
    private _smssLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    get smss$(): Observable<SMS[]> {
        return this._smss.asObservable();
    }

    get sms$(): Observable<SMS> {
        return this._sms.asObservable();
    }

    get smssLength$(): Observable<number> {
        return this._smssLength.asObservable();
    }

    GetSMS(page: number = 1,
           size: number = 10,
           sort: string = '',
           order: 'asc' | 'desc' | '' = 'asc',
           search: string = '')
        : Observable<PagedSMS>
    {
        return this._apiservice.Get<PagedSMS>("sms/list",
            {
                params: { search: search || '', sort, order, page, size}
            })
            .pipe(
                tap((result) => {
                    const data: any = result.data ?? {};

                    // Backend key can be: smsS / smSs / smss / SMSs (depends on JSON naming)
                    const listKey = Object.keys(data).find((k) => k.toLowerCase() === 'smss');
                    const smsList = (listKey ? data[listKey] : []) as SMS[];

                    this._smss.next(smsList ?? []);
                    this._smssLength.next(data?.length ?? 0);
                }),
                map(r => r.data),
                catchError(error => {
                    console.error('Error loading SMS list:', error);
                    this._smss.next([]);
                    this._smssLength.next(0);
                    return of({ smsS: [], length: 0 } as PagedSMS);
                })
            );
    }

    CreateNewSMS(): Observable<SMS> {
        const newSMS: SMS = {
            smsId: 'new',
            nomPrenom: '',
            phoneNumber: '',
            devices: [],
        };
        this._smss.next([newSMS, ...this._smss.value]);
        return of(newSMS);
    }

    AddSMS(sms: SMS): Observable<SMS> {
        const { smsId, ...body } = sms;
        const requestBody = {
            nomPrenom: body.nomPrenom,
            phoneNumber: body.phoneNumber,
            deviceIds: body.devices.map(d => d.deviceId)
        };
        
        return this._apiservice.Post<{ smsId: string }>("sms/create", requestBody).pipe(
            map((v) => {
                const newSMS = { ...sms, smsId: v.data.smsId };
                this._smss.next([newSMS, ...this._smss.value]);
                return newSMS;
            }),
            catchError(error => {
                console.error('Erreur lors de la création du SMS', error);
                throw error;
            })
        );
    }

    UpdateSMS(sms: SMS): Observable<boolean> {
        const requestBody = {
            smsId: sms.smsId,
            nomPrenom: sms.nomPrenom,
            phoneNumber: sms.phoneNumber,
            deviceIds: sms.devices.map(d => d.deviceId)
        };
        
        return this._apiservice.Patch<boolean>("sms/update", requestBody).pipe(
            map((r) => {
                if (r.success) {
                    this._smss.next(this._smss.value.map(s =>
                        s.smsId === sms.smsId ? sms : s
                    ));
                }
                return r.success;
            })
        );
    }

    DeleteSMS(sms: { smsId: string }): Observable<boolean> {
        return this._apiservice.Post<boolean>("sms/delete", sms).pipe(
            map((v) => {
                this._smss.next(this._smss.value.filter(item => item.smsId !== sms.smsId));
                return v.success;
            })
        );
    }

    GetSMSById(Id: string): Observable<SMS> {
        const index = this._smss.value?.findIndex(x => x.smsId === Id);
        return of(this._smss.value[index]);
    }
}
