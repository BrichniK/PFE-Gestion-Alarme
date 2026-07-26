import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedAlerte, Alerte, EmployeePlanning, GroupeWithEmployees } from './alerte.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root',
})
export class AlerteService {
    private _alertes: BehaviorSubject<Alerte[] | null> = new BehaviorSubject(
        []
    );
    private _alerte: BehaviorSubject<Alerte | null> = new BehaviorSubject(null);
    private _alertesLength: BehaviorSubject<number | null> =
        new BehaviorSubject(0);
    private _alertesLengthActif: BehaviorSubject<number | null> =
        new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    private _syncAlertesState(alertes: Alerte[] | null | undefined, totalLength?: number | null): void {
        const nextAlertes = [...(alertes ?? [])];
        this._alertes.next(nextAlertes);
        this._alertesLength.next(totalLength ?? nextAlertes.length);
        this._alertesLengthActif.next(nextAlertes.filter((item) => !item.traiter).length);
    }

    get alertes$(): Observable<Alerte[]> {
        return this._alertes.asObservable();
    }

    get alerte$(): Observable<Alerte> {
        return this._alerte.asObservable();
    }

    get alertesLength$(): Observable<number> {
        return this._alertesLength.asObservable();
    }

    get alertesLengthActif$(): Observable<number> {
        return this._alertesLengthActif.asObservable();
    }

    GetAlerte(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedAlerte> {
        return this._apiservice
            .Get<PagedAlerte>('alerte/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._syncAlertesState(result.data?.alertes, result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewAlerte(): Observable<Alerte> {
        const newAlerte: Alerte = {
            alerteId: 'new',
            date: null,
            dispositifId: null,
            typeId: null,
            traiter: false,
        };
        this._syncAlertesState(
            [newAlerte, ...(this._alertes.value ?? [])],
            (this._alertesLength.value ?? 0) + 1
        );
        return of(newAlerte);
    }

    AddAlerte(alerte: Alerte): Observable<Alerte> {
        const { alerteId, ...body } = alerte;
        return this._apiservice.Post<Alerte>('alerte/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newAlerte = r.data;
                this._syncAlertesState(
                    [newAlerte, ...(this._alertes.value ?? [])],
                    (this._alertesLength.value ?? 0) + 1
                );
                return newAlerte;
            })
        );
    }

    UpdateAlerte(alerte: Alerte): Observable<boolean> {
        return this._apiservice.Patch<boolean>('alerte/update', alerte).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index =
                    this._alertes.value?.findIndex(
                        (item) => item.alerteId === alerte.alerteId
                    ) ?? -1;
                if (index !== -1) {
                    const nextAlertes = [...(this._alertes.value ?? [])];
                    nextAlertes[index] = alerte;
                    this._syncAlertesState(nextAlertes, this._alertesLength.value);
                }
                return true;
            })
        );
    }

    DeleteAlerte(alerte: { alerteId: string }): Observable<boolean> {
        if (alerte.alerteId === 'new') {
            const nextAlertes = [...(this._alertes.value ?? [])];
            const index = nextAlertes.findIndex(
                (item) => item.alerteId === alerte.alerteId
            );
            if (index !== -1) {
                nextAlertes.splice(index, 1);
            }
            this._syncAlertesState(nextAlertes, Math.max(0, (this._alertesLength.value ?? 1) - 1));
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `alerte/${alerte.alerteId}/delete?alerteId=${alerte.alerteId}`,
                alerte
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index =
                        this._alertes.value?.findIndex(
                            (item) => item.alerteId === alerte.alerteId
                        ) ?? -1;
                    if (index !== -1) {
                        const nextAlertes = [...(this._alertes.value ?? [])];
                        nextAlertes.splice(index, 1);
                        this._syncAlertesState(nextAlertes, Math.max(0, (this._alertesLength.value ?? 1) - 1));
                    }
                    return true;
                })
            );
    }

    GetAlerteById(id: string): Observable<Alerte> {
        const index = this._alertes.value?.findIndex((x) => x.alerteId === id);
        return of(this._alertes.value[index]);
    }

    TraiterAlerte(alerteId: string, employeeId: string): Observable<boolean> {
        const previousTraiterState =
            this._alertes.value?.find((item) => item.alerteId === alerteId)?.traiter ?? false;

        this._setAlertTraiterState(alerteId, true);

        this._apiservice
            .Post<boolean>('alerte/traiter', { alerteId, employeeId })
            .pipe(
                map((r) => {
                    if (!r.success || r.data !== true) {
                        throw new Error(r.message || 'Unable to assign alert');
                    }

                    return true;
                })
            )
            .subscribe({
                error: () => {
                    this._setAlertTraiterState(alerteId, previousTraiterState);
                },
            });

        return of(true);
    }

    GetEmployeesByPlanning(
        date: string,
        deviceId: string
    ): Observable<GroupeWithEmployees[]> {
        return this._apiservice
            .Get<GroupeWithEmployees[]>('alerte/employees-by-planning', {
                params: { date, deviceId },
            })
            .pipe(map((r) => r.data ?? []));
    }

    private _setAlertTraiterState(alerteId: string, traiter: boolean): void {
        const index =
            this._alertes.value?.findIndex((item) => item.alerteId === alerteId) ?? -1;

        if (index === -1) {
            return;
        }

        const nextAlertes = [...(this._alertes.value ?? [])];
        nextAlertes[index] = {
            ...nextAlertes[index],
            traiter,
        };

        this._syncAlertesState(nextAlertes, this._alertesLength.value);
    }
}
