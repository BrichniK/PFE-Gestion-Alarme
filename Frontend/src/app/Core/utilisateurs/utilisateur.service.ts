import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, tap, map, catchError } from 'rxjs';
import { PagedUtilisateur, Utilisateur } from './utilisateur.model';
import { ApiService } from '../common/api.service';
import { ApiResponse } from '../common/api-response';
import { Societe  } from '../societe/societe.model';

@Injectable({
    providedIn: 'root'
})
export class UtilisateurService {

    private _utilisateurs: BehaviorSubject<Utilisateur[] | null> = new BehaviorSubject([]);
    private _utilisateur: BehaviorSubject<Utilisateur | null> = new BehaviorSubject(null);
    private _utilisateursLength: BehaviorSubject<number | null> = new BehaviorSubject(0);
    private _societes: BehaviorSubject<Societe [] | null> = new BehaviorSubject(null);

    constructor(private _apiservice: ApiService) {}

    get utilisateurs$(): Observable<Utilisateur[]> {
        return this._utilisateurs.asObservable();
    }

    get utilisateur$(): Observable<Utilisateur> {
        return this._utilisateur.asObservable();
    }

    get utilisateurLength$(): Observable<number> {
        return this._utilisateursLength.asObservable();
    }

    GetUtilisateur(page: number = 1,
                   size: number = 10,
                   sort: string = '',
                   order: 'asc' | 'desc' | '' = 'asc',
                   search: string = '')
        :Observable<PagedUtilisateur>
    {
        return this._apiservice.Get<PagedUtilisateur>("utilisateur/list",
            {
                params: { search: search || '', sort, order, page, size}
            })
            .pipe(
                tap((produits)=>{
                    this._utilisateurs.next(produits.data?.utilisateurs??[]);
                    this._utilisateursLength.next(produits.data?.length);
                }),
                map(r=> r.data)
            );
    }


    GetSocietes(): Observable<Societe []> {
        return this._apiservice.Get<Societe []>('societe/list').pipe(
            map((response) => response.data ?? [])
        );
    }

    CreateNewUtilisateur(): Observable<Utilisateur> {
        const newUtilisateur: Utilisateur = {
            utilisateurId: 'new',
            nomUtilisateur: null,
            nom: null,
            prenom: null,
            email: null,
            password: null,
            roleUtilisateurId:null,
            libelleRoleUtilisateur:null,
            isActive: true,
            etat: null,
            societeId: '',
        };
        this._utilisateurs.next([newUtilisateur, ...this._utilisateurs.value]);
        return of(newUtilisateur);
    }

    AddUtilisateur(utilisateur: Utilisateur): Observable<Utilisateur> {
        return this._apiservice.Post<Utilisateur>("utilisateur/create", utilisateur).pipe(
            map((v) => {
                const newUtilisateur = v.data;
                this._utilisateurs.next([newUtilisateur, ...this._utilisateurs.value]);
                return newUtilisateur;
            }),
            catchError(error => {
                console.error('Erreur lors de la création de l\'utilisateur', error);
                throw error;
            })
        );
    }

    UpdateUtilisateur(utilisateur: Utilisateur): Observable<boolean> {
        return this._apiservice.Patch<boolean>("utilisateur/update", utilisateur).pipe(
            map((r) => {
                if (r.success) {
                    this._utilisateurs.next(this._utilisateurs.value.map(u =>
                        u.utilisateurId === utilisateur.utilisateurId ? utilisateur : u
                    ));
                }
                return r.success;
            })
        );
    }

    DeleteUtilisateur(utilisateur: { utilisateurId: string }): Observable<boolean> {
        return this._apiservice.Post<Utilisateur>("utilisateur/delete", utilisateur).pipe(
            map((v) => {
                this._utilisateurs.next(this._utilisateurs.value.filter(item => item.utilisateurId !== utilisateur.utilisateurId));
                return v.success;
            })
        );
    }

    GetUtilisateurById(Id:string): Observable<Utilisateur>{
        const index = this._utilisateurs.value?.findIndex(x => x.utilisateurId === Id);
        return of(this._utilisateurs.value[index])
    }


    GetRole(): Observable<any> {
        return this._apiservice.Get<any>("utilisateur/role").pipe(
            map((v) => v.data)
        );
    }

    get societes$(): Observable<Societe[] | null> {
        return this._societes.asObservable();
    }
}
