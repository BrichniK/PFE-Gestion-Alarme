import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component, OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { debounceTime, distinctUntilChanged, finalize, map, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import {
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { FuseConfirmationService } from '../../../../../@fuse/services/confirmation';
import { Utilisateur } from '../../../../core/utilisateurs/utilisateur.model';
import { UtilisateurService } from '../../../../core/utilisateurs/utilisateur.service';
import { AsyncPipe, NgClass, NgForOf, NgTemplateOutlet } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { fuseAnimations } from '../../../../../@fuse/animations';
import { RoleNavigation, RoleUtilisateur } from '../../../../core/role-utilisateur/role-utilisateur.model';
import { RoleUtilisateurService } from '../../../../core/role-utilisateur/role-utilisateur.service';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ActivatedRoute } from '@angular/router';
import { FuseNavigationAction } from '../../../../../@fuse/components/navigation';
import { MatAutocomplete, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { PagedSociete, Societe } from '../../../../core/societe/societe.model';
import { SocieteService } from '../../../../core/societe/societe.service';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'app-utilisateur',
    standalone: true,
    imports: [
        AsyncPipe,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatOptionModule,
        MatProgressBarModule,
        MatSelectModule,
        MatSortModule,
        NgTemplateOutlet,
        ReactiveFormsModule,
        NgClass,
        MatPaginatorModule,
        MatSlideToggleModule,
        MatAutocomplete,
        MatAutocompleteTrigger,
        NgForOf,
        TranslocoDirective,
    ],
    templateUrl: './utilisateur.component.html',
    styleUrl: './utilisateur.component.scss',
    encapsulation  : ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations : fuseAnimations
})
export class UtilisateurComponent implements OnInit, OnDestroy{

    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    isChangingPassword = false;
    utilisateur$: Observable<Utilisateur[]>;
    roleUtilisateurs: RoleUtilisateur[]=[];
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    utilisateurslength:number;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    selectedUtilisateur: Utilisateur | null = null;
    selectedUtilisateurForm: UntypedFormGroup;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    roles:any[]=[];
    saveClicked= false
    roleNavigation: RoleNavigation;
    societe: Societe[]=[];
    filteredSocietes$: Observable<Societe[]>;


    constructor(
        private _utilisateurService: UtilisateurService,
        private _activatedRoute: ActivatedRoute,
        private _roleUtilisateurService: RoleUtilisateurService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _formBuilder: UntypedFormBuilder,
        private _societeService : SocieteService,

    ) {
    }

    SortChange(){

        this.closeDetails();
        this.isLoading = true;
        this.getUtilisateurs()
            .pipe(
                map(()=>{
                    this.isLoading=false;

                    // Mark for check
                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }

    getUtilisateurs(){
        return this._utilisateurService.GetUtilisateur(
            (this._paginator?.pageIndex | 0) + 1,
            this._paginator?.pageSize,
            this._sort?.active,
            this._sort?.direction,
            this.searchInputControl.value);
    }

    hasActionPermission(action: FuseNavigationAction): boolean{
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    displaySociete = (societeId: string) => {
        const found = this.societe.find((s) => s.societeId === societeId);
        return found ? found.nom : '';
    };

    ngOnInit(): void {

        // const societeCtrl = this.selectedUtilisateurForm.get('societeId')!;
        //
        // this.filteredSocietes$ = societeCtrl.valueChanges.pipe(
        //     debounceTime(300),
        //     distinctUntilChanged(),
        //     switchMap((value) =>
        //         this._societeService
        //             .GetSociete(1, 20, '', 'asc', value || '')
        //             .pipe(map((res) => res.societes || []))
        //     )
        // );

        this.selectedUtilisateurForm = this._formBuilder.group({
            utilisateurId:[''],
            nomUtilisateur:[null, [Validators.required]],
            nom:[''],
            prenom:[null],
            email:['', [Validators.required]],
            password:[''],
            roleUtilisateurId:[null, [Validators.required]],
            isActive:[true, [Validators.required]],
            societeId: ['']
        });

        this.filteredSocietes$ = this.selectedUtilisateurForm.get('societeId')!.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            switchMap((value) =>
                this._societeService
                    .GetSociete(1, 20, '', 'asc', value || '')
                    .pipe(map((res) => res.societes || []))
            )
        );

        this._activatedRoute.data
            .subscribe(async (data) => {

                if (!data?.navigation){
                    return;
                }

                this.roleNavigation = data.navigation;
            });

        this.utilisateur$ = this._utilisateurService.utilisateurs$;

        this._utilisateurService.utilisateurLength$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((length)=>{
                this.utilisateurslength=length;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this.searchInputControl.valueChanges
            .pipe(
                switchMap((query)=>{
                    this.closeDetails();
                    this.isLoading=true;
                    return this.getUtilisateurs();
                }),
                map(()=>{
                    this.isLoading=false;
                })
            ).subscribe();

        this._roleUtilisateurService.roleUtilisateurs$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((item)=>{
                this.roleUtilisateurs=item;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this._utilisateurService.GetRole().subscribe((roles :any[])=>{
            this.roles=roles;
            this._changeDetectorRef.markForCheck();
        });

        this._societeService.GetSociete().subscribe((pagedSociete: PagedSociete) => {
            // Extract societes from the pagedSociete object
            this.societe = pagedSociete.societes || [];
            this._changeDetectorRef.markForCheck();
        });


    }

    getRoleById(roleUtilisateurId: string): RoleUtilisateur {
        return this.roleUtilisateurs.find(role => role.roleUtilisateurId === roleUtilisateurId);
    }

    toggleChangePassword() {
        this.isChangingPassword = !this.isChangingPassword;
    }

    CreateUtilisateur(){

        if (!this.hasActionPermission(FuseNavigationAction.Add)){
            return;
        }

        this._utilisateurService.CreateNewUtilisateur().subscribe((newUtilisateur)=>{
            this.selectedUtilisateur=newUtilisateur;
            this.selectedUtilisateurForm.patchValue(newUtilisateur);
            this._changeDetectorRef.markForCheck();
        });
    }

    /**
     * Toggle Type Vehicule details
     *
     * @param fonctionId
     */
    toggleDetails(utilisateurId: string): void
    {
        this.isChangingPassword=false;
        // If the product is already selected...
        if ( this.selectedUtilisateur && this.selectedUtilisateur.utilisateurId === utilisateurId )
        {
            // Close the details
            this.closeDetails();
            return;
        }

        // Get the region by id
        this._utilisateurService.GetUtilisateurById(utilisateurId)
            .subscribe((utilisateur) => {

                // Set the selected product
                this.selectedUtilisateur = utilisateur;

                // Fill the form
                this.selectedUtilisateurForm.patchValue(utilisateur);

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });
    }

    /**
     * Close the details
     */
    closeDetails(): void
    {
        this.selectedUtilisateur = null;
        // Mark for check
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Update the selected product using the form data
     */
    SaveSelectedUtilisateur(): void
    {
        if (!this.hasActionPermission(FuseNavigationAction.Edit) && !this.hasActionPermission(FuseNavigationAction.Add)){
            return;
        }

        this.saveClicked = true;

        if (this.selectedUtilisateurForm.invalid) {
            this._changeDetectorRef.markForCheck();
            setTimeout(()=> {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        // Get the product object
        const utilisateur = this.selectedUtilisateurForm.getRawValue();

        if(utilisateur.utilisateurId=== "new" && this.hasActionPermission(FuseNavigationAction.Add)) {
            this._utilisateurService.AddUtilisateur(utilisateur)
                .pipe(
                    finalize(() => {
                        this.saveClicked = false;
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }

        if(utilisateur.utilisateurId!== "new" && this.hasActionPermission(FuseNavigationAction.Edit)) {
            // Update the product on the server
            this._utilisateurService.UpdateUtilisateur(utilisateur)
                .pipe(
                    finalize(() => {
                        this.saveClicked = false;
                        this.closeDetails();
                        this._changeDetectorRef.markForCheck();
                    })
                )
                .subscribe(() => {
                    this.SortChange();
                });
        }
    }

    /**
     * Delete the selected product using the form data
     */
    deleteSelectedUtilisateur(utilisateur: Utilisateur): void
    {
        if (!this.hasActionPermission(FuseNavigationAction.Delete)){
            return;
        }

        // Open the confirmation dialog
        const confirmation = this._fuseConfirmationService.open({
            icon: {
                show: false,
            },
            title: 'Supprimer cette utilisateur',
            message: 'Êtes-vous sûr de vouloir supprimer cette utilisateur? Cette action ne peut pas être annulée!',
            actions: {
                confirm: {
                    label: 'Supprimer'
                },
                cancel:{
                    label:'Annuler'
                }
            }
        });

        // Subscribe to the confirmation dialog closed action
        confirmation.afterClosed().subscribe((result) => {

            // If the confirm button pressed...
            if ( result === 'confirmed' )
            {

                // Delete the Fonction on the server
                this._utilisateurService.DeleteUtilisateur({utilisateurId: utilisateur.utilisateurId})
                    .subscribe(() => {

                        // Close the details
                        this.closeDetails();

                        // Mark for check
                        this._changeDetectorRef.markForCheck();
                    });
            }
        });
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any
    {
        return item.utilisateurId || index;
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}