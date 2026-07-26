import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { Groupe, PagedGroupe } from './groupe.model';
import { ApiService } from '../common/api.service';

interface CreateGroupeApiResponse {
    groupeId: string;
}

@Injectable({
    providedIn: 'root'
})
export class GroupeService {
    private _groupes: BehaviorSubject<Groupe[] | null> = new BehaviorSubject([]);
    private _groupe: BehaviorSubject<Groupe | null> = new BehaviorSubject(null);
    private _groupesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    get groupes$(): Observable<Groupe[]> {
        return this._groupes.asObservable();
    }

    get groupe$(): Observable<Groupe> {
        return this._groupe.asObservable();
    }

    get groupesLength$(): Observable<number> {
        return this._groupesLength.asObservable();
    }

    GetGroupe(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedGroupe> {
        return this._apiservice
            .Get<PagedGroupe>('groupe/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._groupes.next(result.data?.groupes ?? []);
                    this._groupesLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewGroupe(): Observable<Groupe> {
        const newGroupe: Groupe = {
            groupeId: 'new',
            nom: null,
            color: '#2E6C9F',
            employeeIds: [],
        };
        return of(newGroupe);
    }

    AddGroupe(groupe: Groupe): Observable<Groupe> {
        const { groupeId, ...body } = groupe;
        return this._apiservice.Post<CreateGroupeApiResponse>('groupe/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newGroupe: Groupe = {
                    ...body,
                    groupeId: r.data?.groupeId,
                };
                this._groupes.next([newGroupe, ...this._groupes.value ?? []]);
                return newGroupe;
            })
        );
    }

    UpdateGroupe(groupe: Groupe): Observable<boolean> {
        return this._apiservice.Patch<boolean>('groupe/update', groupe).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._groupes.value?.findIndex(
                    (item) => item.groupeId === groupe.groupeId
                ) ?? -1;
                if (index !== -1) {
                    this._groupes.value[index] = groupe;
                    this._groupes.next(this._groupes.value);
                }
                return true;
            })
        );
    }

    DeleteGroupe(groupe: { groupeId: string }): Observable<boolean> {
        if (groupe.groupeId === 'new') {
            const index = this._groupes.value.findIndex(
                (item) => item.groupeId === groupe.groupeId
            );
            this._groupes.value.splice(index, 1);
            this._groupes.next(this._groupes.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `groupe/${groupe.groupeId}/delete?groupeId=${groupe.groupeId}`,
                groupe
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._groupes.value.findIndex(
                        (item) => item.groupeId === groupe.groupeId
                    ) ?? -1;
                    if (index !== -1) {
                        this._groupes.value.splice(index, 1);
                        this._groupes.next(this._groupes.value);
                    }
                    return true;
                })
            );
    }

    GetGroupeById(id: string): Observable<Groupe> {
        const localGroupe = this._groupes.value?.find((x) => x.groupeId === id);
        if (localGroupe) {
            return of(localGroupe);
        }

        return this._apiservice.Get<Groupe>(`groupe/${id}/one`).pipe(
            map((r) => r.data),
            tap((groupe) => {
                if (!groupe) {
                    return;
                }
                const current = this._groupes.value ?? [];
                const exists = current.some((item) => item.groupeId === groupe.groupeId);
                if (!exists) {
                    this._groupes.next([groupe, ...current]);
                }
            })
        );
    }
}
