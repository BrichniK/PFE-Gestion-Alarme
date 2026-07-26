import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { ApiService } from '../common/api.service';
import { JourFerie, PagedJourFerie } from './jour-ferie.model';

interface CreateJourFerieApiResponse {
    jourFerieId: string;
}

@Injectable({
    providedIn: 'root'
})
export class JourFerieService {
    private _joursFeries: BehaviorSubject<JourFerie[] | null> = new BehaviorSubject([]);
    private _jourFerie: BehaviorSubject<JourFerie | null> = new BehaviorSubject(null);
    private _joursFeriesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiService: ApiService) {}

    get joursFeries$(): Observable<JourFerie[]> {
        return this._joursFeries.asObservable();
    }

    get jourFerie$(): Observable<JourFerie> {
        return this._jourFerie.asObservable();
    }

    get joursFeriesLength$(): Observable<number> {
        return this._joursFeriesLength.asObservable();
    }

    GetJourFerie(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedJourFerie> {
        return this._apiService
            .Get<PagedJourFerie>('jour-ferie/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result: any) => {
                    const payload = result?.data ?? result;
                    const joursFeries = payload?.joursFeries ?? payload?.jourFeries ?? [];
                    const length = payload?.length ?? joursFeries.length ?? 0;
                    this._joursFeries.next(joursFeries);
                    this._joursFeriesLength.next(length);
                }),
                map((r: any) => (r?.data ?? r) as PagedJourFerie)
            );
    }

    CreateNewJourFerie(): Observable<JourFerie> {
        const newJourFerie: JourFerie = {
            jourFerieId: 'new',
            date: null,
            label: null,
        };
        this._joursFeries.next([newJourFerie, ...this._joursFeries.value]);
        return of(newJourFerie);
    }

    AddJourFerie(jourFerie: JourFerie): Observable<JourFerie> {
        const { jourFerieId, ...body } = jourFerie;
        return this._apiService.Post<CreateJourFerieApiResponse>('jour-ferie/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newJourFerie: JourFerie = {
                    ...body,
                    jourFerieId: r.data?.jourFerieId,
                };
                this._joursFeries.next([newJourFerie, ...this._joursFeries.value ?? []]);
                return newJourFerie;
            })
        );
    }

    UpdateJourFerie(jourFerie: JourFerie): Observable<boolean> {
        return this._apiService.Patch<boolean>('jour-ferie/update', jourFerie).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._joursFeries.value?.findIndex(
                    (item) => item.jourFerieId === jourFerie.jourFerieId
                ) ?? -1;
                if (index !== -1) {
                    this._joursFeries.value[index] = jourFerie;
                    this._joursFeries.next(this._joursFeries.value);
                }
                return true;
            })
        );
    }

    DeleteJourFerie(jourFerie: { jourFerieId: string }): Observable<boolean> {
        if (jourFerie.jourFerieId === 'new') {
            const index = this._joursFeries.value.findIndex(
                (item) => item.jourFerieId === jourFerie.jourFerieId
            );
            this._joursFeries.value.splice(index, 1);
            this._joursFeries.next(this._joursFeries.value);
            return of(true);
        }

        return this._apiService
            .Post<boolean>(
                `jour-ferie/${jourFerie.jourFerieId}/delete`,
                {}
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._joursFeries.value.findIndex(
                        (item) => item.jourFerieId === jourFerie.jourFerieId
                    ) ?? -1;
                    if (index !== -1) {
                        this._joursFeries.value.splice(index, 1);
                        this._joursFeries.next(this._joursFeries.value);
                    }
                    return true;
                })
            );
    }

    GetJourFerieById(id: string): Observable<JourFerie> {
        const localJourFerie = this._joursFeries.value?.find((x) => x.jourFerieId === id);
        if (localJourFerie) {
            return of(localJourFerie);
        }

        return this._apiService.Get<JourFerie>(`jour-ferie/${id}/one`).pipe(
            map((r) => r.data),
            tap((jourFerie) => {
                if (!jourFerie) {
                    return;
                }
                const current = this._joursFeries.value ?? [];
                const exists = current.some((item) => item.jourFerieId === jourFerie.jourFerieId);
                if (!exists) {
                    this._joursFeries.next([jourFerie, ...current]);
                }
            })
        );
    }
}
