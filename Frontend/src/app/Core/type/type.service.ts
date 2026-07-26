import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedType, Type } from './type.model';
import { ApiService } from '../common/api.service';

@Injectable({
    providedIn: 'root'
})
export class TypeService {
    private _types: BehaviorSubject<Type[] | null> = new BehaviorSubject([]);
    private _type: BehaviorSubject<Type | null> = new BehaviorSubject(null);
    private _typesLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) { }

    get types$(): Observable<Type[]> {
        return this._types.asObservable();
    }

    get type$(): Observable<Type> {
        return this._type.asObservable();
    }

    get typesLength$(): Observable<number> {
        return this._typesLength.asObservable();
    }

    GetType(
        page: number = 1,
        size: number = 10,
        sort: string = '',
        order: 'asc' | 'desc' | '' = 'asc',
        search: string = ''
    ): Observable<PagedType> {
        return this._apiservice
            .Get<PagedType>('type/list', {
                params: { search: search || '', sort, order, page, size },
            })
            .pipe(
                tap((result) => {
                    this._types.next(result.data?.types ?? []);
                    this._typesLength.next(result.data?.length);
                }),
                map((r) => r.data)
            );
    }

    CreateNewType(): Observable<Type> {
        const newType: Type = {
            typeId: 'new',
            code: null,
            label: null,
            dureeNominal: null,
        };
        this._types.next([newType, ...this._types.value]);
        return of(newType);
    }

    AddType(type: Type): Observable<Type> {
        const { typeId, ...body } = type;
        return this._apiservice.Post<Type>('type/add', body).pipe(
            map((r) => {
                if (!r.success) {
                    throw new Error(r.message);
                }
                const newType = r.data;
                this._types.next([newType, ...this._types.value ?? []]);
                return newType;
            })
        );
    }

    UpdateType(type: Type): Observable<boolean> {
        return this._apiservice.Patch<boolean>('type/update', type).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }
                const index = this._types.value?.findIndex(
                    (item) => item.typeId === type.typeId
                ) ?? -1;
                if (index !== -1) {
                    this._types.value[index] = type;
                    this._types.next(this._types.value);
                }
                return true;
            })
        );
    }

    DeleteType(type: { typeId: string }): Observable<boolean> {
        if (type.typeId === 'new') {
            const index = this._types.value.findIndex(
                (item) => item.typeId === type.typeId
            );
            this._types.value.splice(index, 1);
            this._types.next(this._types.value);
            return of(true);
        }

        return this._apiservice
            .Post<boolean>(
                `type/${type.typeId}/delete?typeId=${type.typeId}`,
                type
            )
            .pipe(
                map((r) => {
                    if (!r.success) {
                        return false;
                    }
                    const index = this._types.value.findIndex(
                        (item) => item.typeId === type.typeId
                    ) ?? -1;
                    if (index !== -1) {
                        this._types.value.splice(index, 1);
                        this._types.next(this._types.value);
                    }
                    return true;
                })
            );
    }

    GetTypeById(id: string): Observable<Type> {
        const index = this._types.value?.findIndex((x) => x.typeId === id);
        return of(this._types.value[index]);
    }
}
