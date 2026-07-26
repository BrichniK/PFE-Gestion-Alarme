import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, tap } from 'rxjs';
import { PagedSociete, Societe } from './societe.model';
import { ApiService } from '../common/api.service';

@Injectable({
  providedIn: 'root'
})
export class SocieteService {
    private _societes: BehaviorSubject<Societe[] | null> = new BehaviorSubject([]);
    private _societe: BehaviorSubject<Societe | null> = new BehaviorSubject(null);
    private _societeLength: BehaviorSubject<number | null> = new BehaviorSubject(0);

    constructor(private _apiservice: ApiService) { }

    get societes$(): Observable<Societe[] > {
        return this._societes.asObservable();
    }


    get societe$(): Observable<Societe> {
        return this._societe.asObservable();
    }
    get societesLength$():Observable<number>{
        return this._societeLength.asObservable();
    }

    GetSociete(page:number = 1,
               size:number = 1000,
               sort: string = 'raisonSociale',
               order: 'asc' | 'desc' | '' = 'asc',
               search: string = ''
    ):Observable<PagedSociete>
    {
        return this._apiservice.Get<PagedSociete>("societe/list",{params:{search:search || '', sort, order, page, size}})
            .pipe(
                tap((societes)=>{
                    this._societes.next(societes.data?.societes);
                    this._societeLength.next(societes.data?.length);
                }),
                map(r=> r.data)
            );
    }

    CreateNewSociete():Observable<Societe> {

        this._societe.next(null);

        const newSociete: Societe = {
            societeId: null,
            logoPath:"",
            logoData:"",
            logoExtension:"",
            nom:"",
            matriculeFiscal:"",
            rne:"",
            capital:null,
            dateOverture:null,
            telephone1:"",
            telephone2:"",
            fax1:"",
            fax2:"",
            email:"",
            adresse:"",
            codeSociete:""
        }
        this._societe.next(newSociete);

        return of(newSociete);
    }

    AddSociete(societe:Societe) :Observable<Societe>
    {
        return this._apiservice.Post<Societe>("societe/add",societe)
            .pipe(
                map((r)=>{
                    if (!r.success){
                        throw new Error(r.message);
                    }
                    societe.societeId=r.data.societeId;
                    this._societes.next([r.data, ...this._societes.value??[]])

                    return r.data;
                })
            );
    }

    UpdateSociete(societe:Societe)
        :Observable<boolean>
    {
        return this._apiservice.Patch<boolean>("societe/update", societe)
            .pipe(
                map((r)=>{
                    if(!r.success) {
                        return false;
                    }

                    const index = this._societes.value
                        .findIndex(item => item.societeId === societe.societeId)??-1;

                    if (index === -1){
                        return true;
                    }

                    this._societes.value[index] = societe;
                    this._societes.next(this._societes.value);

                    return true;
                })
            );
    }

    DeleteSociete(societe: { societeId:string })
        :Observable<boolean>
    {
        //Delete locally only if added only locally
        if (societe.societeId===null){
            const index = this._societes.value
                .findIndex(item => item.societeId === societe.societeId);

            this._societes.value.splice(index, 1);
            this._societes.next(this._societes.value);

            return of(true);
        }

        return this._apiservice.Post<boolean>(
            `societe/${societe.societeId}/delete?societeId=${societe.societeId}`,
            societe // Ajout du corps de la requête
        ).pipe(
            map((r) => {
                if (!r.success) {
                    return false;
                }

                const index =
                    this._societes.value.findIndex(item => item.societeId === societe.societeId) ?? -1;

                if (index === -1) {
                    return true;
                }

                this._societes.value.splice(index, 1);
                this._societes.next(this._societes.value);

                return true;
            })
        );
    }
    GetSocieteById(societeId:string):Observable<Societe> {
        this._societe.next(null);
        return this._apiservice.Get<Societe>(`societe/${societeId}/one`)
            .pipe(
                map((result) => {
                    if (!result.success){
                        throw new Error("Error accrued while retrieving Societe")
                    }
                    const societe = result.data;
                    societe.societeId = societe.societeId;

                    this._societe.next(societe);

                    return result.data;
                })
            );
    }
}
