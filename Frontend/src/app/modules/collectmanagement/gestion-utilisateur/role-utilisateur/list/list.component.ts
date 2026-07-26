import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { debounceTime, finalize, map, of, Subject, switchMap, takeUntil } from 'rxjs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FuseConfirmationService } from '../../../../../../@fuse/services/confirmation';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule, MatRippleModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { fuseAnimations } from '../../../../../../@fuse/animations';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RoleNavigation, RoleUtilisateur } from '../../../../../core/role-utilisateur/role-utilisateur.model';
import { RoleUtilisateurService } from '../../../../../core/role-utilisateur/role-utilisateur.service';
import { FuseNavigationAction } from '../../../../../../@fuse/components/navigation';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'role-utilisateur-list',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatDatepickerModule,
        MatDividerModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatOptionModule,
        MatRippleModule,
        MatSelectModule,
        MatPaginatorModule,
        MatSortModule,
        RouterLink,
        MatSidenavModule,
        CommonModule,
        MatCheckboxModule,
        MatExpansionModule,
        MatTooltipModule,
        TranslocoDirective,
    ],
    templateUrl: './list.component.html',
    styleUrl: './list.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations : fuseAnimations
})
export class RoleUtilisateurListComponent implements OnInit, OnDestroy {

    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    roleUtilisateurs: RoleUtilisateur[];
    roleUtilisateursLength:number;
    isLoading: boolean = false;
    roleUtilisateurFilterForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roleNavigation: RoleNavigation;

    constructor(
        private _roleUtilisateurService: RoleUtilisateurService,
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _changeDetectorRef: ChangeDetectorRef,
        private _formBuilder: UntypedFormBuilder,
        private _fuseConfirmationService: FuseConfirmationService,
    ) { }

    getRoleUtilisateurs(){
        return this._roleUtilisateurService.GetRoleUtilisateur(
            this.roleUtilisateurFilterForm.get('search').value,
            this._sort.active,
            this._sort.direction,
            this._paginator?.pageIndex,
            this._paginator?.pageSize);
    }

    SortChange(){
        this.isLoading = true;
        this.getRoleUtilisateurs()
            .pipe(
                finalize(()=>{
                    this.isLoading=false;
                    // Mark for check
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    hasActionPermission(action: FuseNavigationAction): boolean{
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    ngOnInit(): void {

        this.roleUtilisateurFilterForm = this._formBuilder.group({
            search:['']
        });

        this._activatedRoute.data
            .subscribe(async (data) => {

                if (!data?.navigation){
                    return;
                }

                this.roleNavigation = data.navigation;
            });

        this._roleUtilisateurService.roleUtilisateursLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length)=>{
                this.roleUtilisateursLength=length;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this.roleUtilisateurFilterForm.valueChanges
            .pipe(
                takeUntil(this._unsubscribeAll),
                debounceTime(600),
                switchMap(()=>{

                    if (this.roleUtilisateurFilterForm.invalid) return of(null);
                    this.isLoading=true;
                    return this.getRoleUtilisateurs();
                }),
                map(()=>{
                    this.isLoading=false;
                })
            ).subscribe();

        this._roleUtilisateurService.roleUtilisateurs$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((result)=>{
                this.roleUtilisateurs=result;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

    }

    deleteRoleUtilisateur(roleUtilisateurId:string){

        if (!this.hasActionPermission(FuseNavigationAction.Delete)){
            return;
        }

        // Open the confirmation dialog
        const confirmation = this._fuseConfirmationService.open({
            icon: {
                show: false,
            },
            title  : 'Supprimer ce rôle?',
            message: 'Êtes-vous sûr de vouloir supprimer ce rôle?',
            actions: {
                confirm: {
                    label: 'Supprimer',
                    color: 'warn',
                },
                cancel:{
                    label:'Annuler'
                }
            }
        });

        confirmation.afterClosed().subscribe((result) => {

            // If the confirmed button pressed...
            if ( result !== 'confirmed' )
            {
                return;
            }

            this._roleUtilisateurService.DeleteRoleUtilisateur(roleUtilisateurId)
                .subscribe(result =>{
                    if (result){
                        this.SortChange();
                    }
                });
        });

    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
