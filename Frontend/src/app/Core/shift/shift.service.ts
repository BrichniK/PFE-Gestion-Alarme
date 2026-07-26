import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedShift, Shift } from './shift.model';
import { ApiService } from '../common/api.service';

interface CreateShiftApiResponse {
    shiftId: string;
}

@Injectable({
    providedIn: 'root'
})
export class ShiftService {
    private _shifts: BehaviorSubject<Shift[] | null> = new BehaviorSubject([]);
    private _shift: BehaviorSubject<Shift | null> = new BehaviorSubject(null);
    private _shiftsLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) {}

    get shifts$(): Observable<Shift[]> {
        return this._shifts.asObservable();
    }

    get shift$(): Observable<Shift> {
        return this._shift.asObservable();
    }

    get shiftsLength$(): Observable<number> {
        return this._shiftsLength.asObservable();
    }

    GetShift(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedShift> {
        return this._apiservice
            .Get<PagedShift>('shift/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._shifts.next(result.data?.shifts ?? []);
                    this._shiftsLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewShift(): Observable<Shift> {
        const newShift: Shift = {
            shiftId: 'new',
            label: null,
            startTime: null,
            endTime: null,
        };
        this._shifts.next([newShift, ...this._shifts.value]);
        return of(newShift);
    }

    private formatTime(time: string): string {
        if (!time) return time;
        // "HH:mm" -> "HH:mm:ss" for .NET TimeOnly
        return time.length === 5 ? `${time}:00` : time;
    }

    AddShift(shift: Shift): Observable<Shift> {
        const { shiftId, ...body } = shift;
        body.startTime = this.formatTime(body.startTime);
        body.endTime = this.formatTime(body.endTime);
        return this._apiservice.Post<CreateShiftApiResponse>('shift/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newShift: Shift = {
                    ...body,
                    shiftId: r.data?.shiftId,
                };
                this._shifts.next([newShift, ...this._shifts.value ?? []]);
                return newShift;
            })
        );
    }

    UpdateShift(shift: Shift): Observable<boolean> {
        const payload = {
            ...shift,
            startTime: this.formatTime(shift.startTime),
            endTime: this.formatTime(shift.endTime),
        };
        return this._apiservice.Patch<boolean>('shift/update', payload).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._shifts.value?.findIndex(
                    (item) => item.shiftId === shift.shiftId
                ) ?? -1;
                if (index !== -1) {
                    this._shifts.value[index] = shift;
                    this._shifts.next(this._shifts.value);
                }
                return true;
            })
        );
    }

    DeleteShift(shift: { shiftId: string }): Observable<boolean> {
        if (shift.shiftId === 'new') {
            const index = this._shifts.value.findIndex(
                (item) => item.shiftId === shift.shiftId
            );
            this._shifts.value.splice(index, 1);
            this._shifts.next(this._shifts.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `shift/${shift.shiftId}/delete?shiftId=${shift.shiftId}`,
                shift
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._shifts.value.findIndex(
                        (item) => item.shiftId === shift.shiftId
                    ) ?? -1;
                    if (index !== -1) {
                        this._shifts.value.splice(index, 1);
                        this._shifts.next(this._shifts.value);
                    }
                    return true;
                })
            );
    }

    GetShiftById(id: string): Observable<Shift> {
        const localShift = this._shifts.value?.find((x) => x.shiftId === id);
        if (localShift) {
            return of(localShift);
        }

        return this._apiservice.Get<Shift>(`shift/${id}/one`).pipe(
            map((r) => r.data),
            tap((shift) => {
                if (!shift) {
                    return;
                }
                const current = this._shifts.value ?? [];
                const exists = current.some((item) => item.shiftId === shift.shiftId);
                if (!exists) {
                    this._shifts.next([shift, ...current]);
                }
            })
        );
    }
}
