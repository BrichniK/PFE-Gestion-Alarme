import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedPlanning, Planning } from './planning.model';
import { ApiService } from '../common/api.service';
import { ApiResponse } from '../common/api-response';

@Injectable({
    providedIn: 'root'
})
export class PlanningService {
    private _plannings: BehaviorSubject<Planning[] | null> = new BehaviorSubject([]);
    private _planning: BehaviorSubject<Planning | null> = new BehaviorSubject(null);
    private _planningsLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    private toIdArray(value: unknown): string[] {
        if (Array.isArray(value)) {
            return value
                .map((item) => {
                    if (item === null || item === undefined) {
                        return '';
                    }
                    if (typeof item === 'object') {
                        const obj = item as Record<string, unknown>;
                        return String(
                            obj['groupeId'] ??
                            obj['employeeId'] ??
                            obj['shiftId'] ??
                            obj['deviceId'] ??
                            obj['id'] ??
                            ''
                        ).trim();
                    }
                    return String(item).trim();
                })
                .filter((id) => id.length > 0);
        }

        if (typeof value === 'string') {
            return value
                .split(',')
                .map((id) => id.trim())
                .filter((id) => id.length > 0);
        }

        if (value === null || value === undefined) {
            return [];
        }

        return [String(value).trim()].filter((id) => id.length > 0);
    }

    private normalizePlanning(planning: Partial<Planning> | null | undefined): Planning {
        const raw = (planning ?? {}) as Record<string, unknown>;
        const groupes = planning?.groupes ?? [];
        const devices = planning?.devices ?? [];
        const shifts = planning?.shifts ?? [];

        const groupeIds = this.toIdArray(
            raw['groupeIds'] ?? raw['groupeId'] ?? groupes.map((x) => x.groupeId)
        );
        const employeeIds = this.toIdArray(
            raw['employeeIds'] ?? raw['employeeId'] ?? []
        );
        const deviceIds = this.toIdArray(
            raw['deviceIds'] ?? raw['deviceId'] ?? devices.map((x) => x.deviceId)
        );
        const shiftIds = this.toIdArray(
            raw['shiftIds'] ?? raw['shiftId'] ?? shifts.map((x) => x.shiftId)
        );

        return {
            planningId: planning?.planningId ?? '',
            date: planning?.date ?? null,
            assignmentMode: (raw['assignmentMode'] === 'employee' ? 'employee' : (employeeIds.length > 0 ? 'employee' : 'group')),
            groupeIds,
            employeeIds,
            deviceIds,
            shiftIds,
            groupes,
            devices,
            shifts,
        };
    }

    get plannings$(): Observable<Planning[]> {
        return this._plannings.asObservable();
    }

    get planning$(): Observable<Planning> {
        return this._planning.asObservable();
    }

    get planningsLength$(): Observable<number> {
        return this._planningsLength.asObservable();
    }

    GetPlanning(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedPlanning> {
        return this._apiservice
            .Get<PagedPlanning>('planning/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    const mappedPlannings = (result.data?.plannings ?? []).map((planning) =>
                        this.normalizePlanning(planning)
                    );
                    this._plannings.next(mappedPlannings);
                    this._planningsLength.next(result.data?.length ?? 0);
                }),
                map((r) => ({
                    plannings: (r.data?.plannings ?? []).map((planning) =>
                        this.normalizePlanning(planning)
                    ),
                    length: r.data?.length ?? 0,
                }))
            );
    }

    CreateNewPlanning(): Observable<Planning> {
        const newPlanning: Planning = {
            planningId: 'new',
            date: null,
            groupeIds: [],
            employeeIds: [],
            deviceIds: [],
            shiftIds: [],
            groupes: [],
            devices: [],
            shifts: [],
        };
        return of(newPlanning);
    }

    AddPlanning(planning: Planning): Observable<Planning> {
        const { planningId, ...body } = planning;
        return this._apiservice.Post<{ planningId?: string; planningIds?: string[] }>('planning/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const requestedDates = (planning.dates?.length ? planning.dates : (planning.date ? [planning.date] : []))
                    .map((date) => String(date));
                const planningIds = r.data?.planningIds?.length
                    ? r.data.planningIds
                    : (r.data?.planningId ? [r.data.planningId] : []);

                const newPlannings = requestedDates.length > 0
                    ? requestedDates.map((date, index) => this.normalizePlanning({
                        ...planning,
                        date,
                        planningId: planningIds[index] ?? planningIds[0] ?? planning.planningId,
                    }))
                    : [this.normalizePlanning({
                        ...planning,
                        planningId: planningIds[0] ?? planning.planningId,
                    })];

                this._plannings.next([
                    ...(newPlannings ?? []),
                    ...(this._plannings.value ?? [])
                ]);
                return newPlannings[0];
            })
        );
    }

    UpdatePlanning(planning: Planning): Observable<boolean> {
        return this._apiservice.Patch<boolean>('planning/update', planning).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._plannings.value?.findIndex(
                    (item) => item.planningId === planning.planningId
                ) ?? -1;
                if (index !== -1) {
                    this._plannings.value[index] = planning;
                    this._plannings.next(this._plannings.value);
                }
                return true;
            })
        );
    }

    DeletePlanning(planning: { planningId: string }): Observable<boolean> {
        if (planning.planningId === 'new') {
            const index = this._plannings.value.findIndex(
                (item) => item.planningId === planning.planningId
            );
            this._plannings.value.splice(index, 1);
            this._plannings.next(this._plannings.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `planning/${planning.planningId}/delete`,
                {}
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._plannings.value.findIndex(
                        (item) => item.planningId === planning.planningId
                    ) ?? -1;
                    if (index !== -1) {
                        this._plannings.value.splice(index, 1);
                        this._plannings.next(this._plannings.value);
                    }
                    return true;
                })
            );
    }

    GetPlanningById(id: string): Observable<Planning | null> {
        const foundPlanning = this._plannings.value?.find((x) => x.planningId === id);
        if (foundPlanning) {
            return of(foundPlanning);
        }

        return this._apiservice.Get<Planning>(`planning/${id}/one`).pipe(
            map((response: ApiResponse<Planning>) =>
                response?.data ? this.normalizePlanning(response.data) : null
            )
        );
    }
}
