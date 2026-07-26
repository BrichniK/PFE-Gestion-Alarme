import { inject, Injectable } from '@angular/core';
import { ApiService } from '../common/api.service';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { PagedRoleUtilisateur, RoleUtilisateur } from './role-utilisateur.model';
import { EnumValue } from '../common/enum.model';

@Injectable({
  providedIn: 'root'
})
export class RoleUtilisateurService {
    private urlPrefix= 'role-utilisateur'
    private _apiservice = inject(ApiService);

    private _roleUtilisateurs: BehaviorSubject<RoleUtilisateur[] | null> = new BehaviorSubject([]);
    private _roleUtilisateur: BehaviorSubject<RoleUtilisateur | null> = new BehaviorSubject(null);
    private _roleUtilisateursLength: BehaviorSubject<number> = new BehaviorSubject(0);
    private _actions: BehaviorSubject<EnumValue[] | null> = new BehaviorSubject([]);

    get roleUtilisateurs$():Observable<RoleUtilisateur[]>{
        return this._roleUtilisateurs.asObservable();
    }

    get roleUtilisateur$():Observable<RoleUtilisateur>{
        return this._roleUtilisateur.asObservable();
    }

    get roleUtilisateursLength$():Observable<number>{
        return this._roleUtilisateursLength.asObservable();
    }

    get actions$():Observable<EnumValue[]>{
        return this._actions.asObservable();
    }

    GetRoleUtilisateur(search: string=null,
                       sort: string="LibelleRoleUtilisateur",
                       order: string='asc',
                       page: number = 0,
                       size: number = 10)
        :Observable<PagedRoleUtilisateur>
    {
        const params: any={
            search: search??'',
            sort,
            order,
            page,
            size
        };

        return this._apiservice.Get<PagedRoleUtilisateur>(`${this.urlPrefix}/list`,
            {
                params: params
            })
            .pipe(
                tap((result)=>{
                    this._roleUtilisateurs.next(result.data.rolesUtilisateur);
                    this._roleUtilisateursLength.next(result.data.length);
                }),
                map(r=> r.data)
            );
    }

    GetAllRoleUtilisateur()
        :Observable<RoleUtilisateur[]>
    {
        return this._apiservice.Get<RoleUtilisateur[]>(`${this.urlPrefix}/all`)
            .pipe(
                tap((result)=>{
                    this._roleUtilisateurs.next(result.data);
                }),
                map(r=> r.data)
            );
    }

    CreateNewRoleUtilisateur(): Observable<RoleUtilisateur>{
        const roleUtilisateur: RoleUtilisateur = {
            roleUtilisateurId: null,
            libelleRoleUtilisateur: null,
            navigations: []
        };

        this._roleUtilisateur.next(roleUtilisateur);
        return this._roleUtilisateur;
    }

    GetOneRoleUtilisateur(roleUtilisateurId:string)
        : Observable<RoleUtilisateur>{
        return this._apiservice.Get<RoleUtilisateur>(`${this.urlPrefix}/${roleUtilisateurId}/one`)
            .pipe(
                map((result) =>{

                    if (!result.success){
                        throw new Error("Error accrued while retrieving roleUtilisateur");
                    }

                    this._roleUtilisateur.next(result.data);

                    return result.data;
                })
            );
    }

    AddRoleUtilisateur(roleUtilisateur: RoleUtilisateur)
        :Observable<{roleUtilisateurId:string}>
    {
        return this._apiservice.Post<{roleUtilisateurId:string}>(`${this.urlPrefix}`, roleUtilisateur)
            .pipe(
                map((r)=>{
                    return r.data;
                })
            );
    }

    UpdateRoleUtilisateur(roleUtilisateur: RoleUtilisateur)
        :Observable<boolean>
    {
        return this._apiservice.Patch<boolean>(`${this.urlPrefix}`, roleUtilisateur)
            .pipe(
                map((r)=>{

                    return r.success;
                })
            );
    }

    DeleteRoleUtilisateur(roleUtilisateurId: string): Observable<boolean>
    {
        // Option 1: Send the ID in the request body
        const requestData = { id: roleUtilisateurId };

        return this._apiservice.Post<boolean>(`${this.urlPrefix}/${roleUtilisateurId}/delete`, requestData)
            .pipe(
                map((r)=>{
                    return r.success;
                })
            );
    }

    GetAction()
        :Observable<EnumValue[]>
    {
        return this._apiservice.Get<EnumValue[]>(`${this.urlPrefix}/actions`)
            .pipe(
                tap((response)=>{
                    this._actions.next(response.data);
                }),
                map(r=> r.data)
            );
    }
}
