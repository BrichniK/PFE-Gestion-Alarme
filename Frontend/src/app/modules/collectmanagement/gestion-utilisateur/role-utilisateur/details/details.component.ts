import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    inject,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { finalize, Subject, takeUntil } from 'rxjs';
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder } from '@angular/forms';
import { fuseAnimations } from '../../../../../../@fuse/animations';
import { FuseConfirmationService } from '../../../../../../@fuse/services/confirmation';
import { fromPromise } from 'rxjs/internal/observable/innerFrom';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatOptionModule, MatPseudoCheckbox, MatPseudoCheckboxModule, MatRippleModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { FuseUtilsService } from '../../../../../../@fuse/services/utils';
import { RoleNavigation, RoleUtilisateur } from '../../../../../core/role-utilisateur/role-utilisateur.model';
import { RoleUtilisateurService } from '../../../../../core/role-utilisateur/role-utilisateur.service';
import { NavigationService } from '../../../../../core/navigation/navigation.service';
import { Navigation } from '../../../../../core/navigation/navigation.types';
import {
    FuseNavigationAction,
    FuseNavigationItem,
    FuseNavigationService,
} from '../../../../../../@fuse/components/navigation';
import { EnumValue } from '../../../../../core/common/enum.model';
import { cloneDeep } from 'lodash-es';
import { TranslocoDirective } from '@ngneat/transloco';

@Component({
    selector: 'role-utilisateur-details',
    standalone: true,
    imports: [
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        ReactiveFormsModule,
        CommonModule,
        MatTooltipModule,
        MatDatepickerModule,
        MatOptionModule,
        MatSelectModule,
        MatRippleModule,
        MatPseudoCheckboxModule,
        RouterLink,
        FormsModule,
        TranslocoDirective,
    ],
    templateUrl: './details.component.html',
    styleUrl: './details.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations : fuseAnimations
})
export class RoleUtilisateurDetailsComponent implements OnInit, OnDestroy{

    private _formBuilder = inject(UntypedFormBuilder);
    private _fuseConfirmationService = inject(FuseConfirmationService);
    private _changeDetectorRef = inject(ChangeDetectorRef);
    private _fuseUtilsService = inject(FuseUtilsService);
    private _activatedRoute = inject(ActivatedRoute);
    private _router = inject(Router);
    private _roleUtilisateurService = inject(RoleUtilisateurService);
    private _navigationService = inject(NavigationService);
    private _fuseNavigationService = inject(FuseNavigationService);

    roleUtilisateur: RoleUtilisateur;
    navigation: Navigation;
    flatNavigation: FuseNavigationItem[];
    actions: EnumValue[] = [];
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    saveClicked= false;
    roleNavigation: RoleNavigation;

    hasActionPermission(action: FuseNavigationAction): boolean{
        return !this.roleNavigation || this.roleNavigation?.actions?.includes(action);
    }

    ngOnInit(): void {

        this._roleUtilisateurService.roleUtilisateur$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((item)=>{
                this.roleUtilisateur=item;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this._activatedRoute.data
            .subscribe(async (data) => {

                if (!data?.navigation){
                    return;
                }

                this.roleNavigation = data.navigation;
            });

        this._roleUtilisateurService.actions$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((item)=>{
                this.actions=item;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this._navigationService.navigation$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((item)=>{
                this.navigation=item;

                this.flatNavigation = this._fuseNavigationService.getFlatNavigation(item.default);

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

    }

    isActionExists(navigationItem: FuseNavigationItem, action: number): boolean {
        return navigationItem.action?.includes(action);
    }

    isActionChecked(navigationItem: FuseNavigationItem, action: number): boolean {
        return this.roleUtilisateur?.navigations
            ?.find(f=>f.navigationId===navigationItem.id)
            ?.actions.includes(action);
    }

    isSectionActionChecked(parent: FuseNavigationItem, section: FuseNavigationItem, action: number): boolean {
        return this.roleUtilisateur?.navigations
            ?.find(f=>f.navigationId===parent.id)
            ?.sections
            ?.find(f=>f.sectionId===section.id)
            ?.actions?.includes(action);
    }

    isNavigationItemExists(navigationItem: FuseNavigationItem): boolean {
        return this.roleUtilisateur?.navigations
            ?.findIndex(f=>f.navigationId===navigationItem.id) > -1;
    }

    isNavigationSectionExists(parent: FuseNavigationItem, section: FuseNavigationItem): boolean {
        return this.roleUtilisateur?.navigations
            ?.find(f=>f.navigationId===parent.id)
            ?.sections
            ?.findIndex(f=>f.sectionId===section.id) > -1;
    }

    selectAllState(): 'checked' | 'unchecked' | 'indeterminate' {

        const selectedLength = this.roleUtilisateur?.navigations?.length??0;

        if (selectedLength===0){
            return 'unchecked';
        }

        const length = this.flatNavigation?.length??0;

        if (selectedLength < length) {
            return 'indeterminate';
        }

        let allSelected = true;

        this.flatNavigation?.forEach(navigationItem=>{
            if (!this.isNavigationItemExists(navigationItem)){
                allSelected = false;
                return;
            }
            navigationItem.action?.forEach(action=>{
                if (!this.isActionChecked(navigationItem, action)){
                    allSelected = false;
                    return;
                }
            });
            navigationItem.section?.forEach(section=>{
                if (!this.isNavigationSectionExists(navigationItem, section)){
                    allSelected = false;
                    return;
                }
                section.action?.forEach(action=>{
                    if (!this.isSectionActionChecked(navigationItem, section, action)){
                        allSelected = false;
                        return;
                    }
                });
            });
        });

        return allSelected ? 'checked' : 'indeterminate';
    }

    selectAllClicked(selectAll: MatPseudoCheckbox) {

        if (selectAll.state === 'checked' || selectAll.state === 'indeterminate') {
            this.roleUtilisateur.navigations = [];
            this._changeDetectorRef.markForCheck();
            return;
        }

        this.roleUtilisateur.navigations = [];

        this.flatNavigation?.forEach(navigationItem=>{

            this.roleUtilisateur
                .navigations
                ?.push({
                    navigationId: navigationItem.id,
                    actions:cloneDeep(navigationItem.action??[]),
                    sections:navigationItem.section?.map(section=>({
                        sectionId: section.id,
                        actions:cloneDeep(section.action??[])
                    }))??[]
                });
        });

        this._changeDetectorRef.markForCheck();
    }

    selectRowState(navigationItem: FuseNavigationItem): 'checked' | 'unchecked' | 'indeterminate' {

        const state: ('checked' | 'unchecked' | 'indeterminate')[] = [];

        state.push(this.isNavigationItemExists(navigationItem) ? 'checked' : 'unchecked');

        navigationItem.action?.forEach(action=>{
            state.push(this.isActionChecked(navigationItem, action) ? 'checked' : 'unchecked');
        });

        navigationItem.section?.forEach(section=>{
            state.push(this.isNavigationSectionExists(navigationItem, section) ? 'checked' : 'unchecked');

            section.action?.forEach(action=>{
                state.push(this.isSectionActionChecked(navigationItem, section, action) ? 'checked' : 'unchecked');
            });
        });

        return state.every(f=>f==='checked') ? 'checked'
            : state.every(f=>f==='unchecked') ? 'unchecked'
                : 'indeterminate';
    }

    selectRow(navigationItem: FuseNavigationItem, selectRow: MatPseudoCheckbox) {

        const rowIndex = this.roleUtilisateur.navigations
            ?.findIndex(f=>f.navigationId===navigationItem.id);

        if (rowIndex > -1){
            this.roleUtilisateur.navigations?.splice(rowIndex, 1);
        }

        if (selectRow.state === 'unchecked') {
            this.roleUtilisateur
                .navigations
                ?.push({
                    navigationId: navigationItem.id,
                    actions: cloneDeep(navigationItem.action??[]),
                    sections:navigationItem.section?.map(section=>({
                        sectionId: section.id,
                        actions:cloneDeep(section.action??[])
                    }))??[]
                });
        }

        navigationItem.section?.forEach(section=>{
            const sections = this.roleUtilisateur.navigations
                ?.find(f=>f.navigationId===navigationItem.id)
                ?.sections;

            const sectionIndex = sections
                ?.findIndex(f=>f.sectionId===section.id);

            if (sectionIndex > -1){
                sections?.splice(sectionIndex, 1);
            }
            if (selectRow.state === 'unchecked') {
                sections?.push({ sectionId: section.id, actions: cloneDeep(section.action??[]) });
            }
        });

        this._changeDetectorRef.markForCheck();
    }

    selectAction(navigationItem: FuseNavigationItem, selectAction: MatPseudoCheckbox, action: number){
        const rowIndex = this.roleUtilisateur.navigations
            ?.findIndex(f=>f.navigationId===navigationItem.id);

        if (rowIndex === -1){
            this.roleUtilisateur.navigations.push({ navigationId: navigationItem.id, actions: [ action] });
            this._changeDetectorRef.markForCheck();
            return;
        }

        const actions = this.roleUtilisateur.navigations[rowIndex].actions;
        const actionIndex = actions?.findIndex(f=>f===action);
        if (actionIndex > -1){
            actions.splice(actionIndex, 1);
        }
        if (selectAction.state === 'unchecked') {
            actions.push(action);
        }

        this._changeDetectorRef.markForCheck();
    }

    selectSection(parent: FuseNavigationItem, section: FuseNavigationItem){
        const rowIndex = this.roleUtilisateur.navigations
            ?.findIndex(f=>f.navigationId===parent.id);

        if (rowIndex === -1){
            this.roleUtilisateur
                .navigations?.push({
                navigationId: parent.id,
                actions: [],
                sections:[
                    {
                        sectionId: section.id,
                        actions: cloneDeep(section.action??[])
                    }
                ]
            });

            this._changeDetectorRef.markForCheck();
            return;
        }

        const sections = this.roleUtilisateur.navigations
            ?.find(f=>f.navigationId===parent.id)
            ?.sections;

        const sectionIndex = sections
            ?.findIndex(f=>f.sectionId===section.id);

        if (sectionIndex > -1){
            sections?.splice(sectionIndex, 1);
        }else{
            sections?.push({ sectionId: section.id, actions: cloneDeep(section.action??[]) });
        }

        this._changeDetectorRef.markForCheck();
    }

    selectSectionAction(parent: FuseNavigationItem, section: FuseNavigationItem, action: number){

        const rowIndex = this.roleUtilisateur.navigations
            ?.findIndex(f=>f.navigationId===parent.id);

        if (rowIndex === -1){
            this.roleUtilisateur
                .navigations?.push({
                navigationId: parent.id,
                actions: [],
                sections:[
                    {
                        sectionId: section.id,
                        actions: [action]
                    }
                ]
            });

            this._changeDetectorRef.markForCheck();
            return;
        }

        const sections = this.roleUtilisateur.navigations[rowIndex].sections??[];

        const sectionIndex = sections?.findIndex(f=>f.sectionId===section.id);

        if (sectionIndex === -1){
            sections.push({
                sectionId: section.id,
                actions: [ action]
            });

            this._changeDetectorRef.markForCheck();
            return;
        }

        const actions = sections[sectionIndex].actions;

        const actionIndex = actions?.findIndex(f=>f===action);

        if (actionIndex > -1){
            actions.splice(actionIndex, 1);
        }else{
            actions.push(action);
        }

        this._changeDetectorRef.markForCheck();
    }

    save(): void {

        if ( (!this.hasActionPermission(FuseNavigationAction.Edit) && !!this.roleUtilisateur.roleUtilisateurId) ||
            (!this.hasActionPermission(FuseNavigationAction.Add) && !this.roleUtilisateur.roleUtilisateurId)){
            return;
        }

        this.saveClicked = true;

        if (!this.roleUtilisateur?.libelleRoleUtilisateur || this.roleUtilisateur?.navigations?.length === 0) {
            this._changeDetectorRef.markForCheck();
            setTimeout(()=> {
                this.saveClicked = false;
                this._changeDetectorRef.markForCheck();
            }, 500);
            return;
        }

        const confirmation = this._fuseConfirmationService.open({
            title  : 'Enregistrer',
            message: "Enregistrement de rôle utilisateur. Voulez-vous continuer?",
            actions: {
                confirm: {
                    label: 'Enregistrer',
                    color:"primary"
                }
            },
            icon:{
                show:false
            },
            dismissible:true
        });

        // Subscribe to the confirmation dialog closed action
        confirmation.afterClosed()
            .subscribe((result) => {

                if (result !== 'confirmed') {
                    return;
                }

                const roleUtilisateur = this.roleUtilisateur;

                if (!this.roleUtilisateur.roleUtilisateurId) {

                    this._roleUtilisateurService
                        .AddRoleUtilisateur(roleUtilisateur)
                        .pipe(
                            finalize(() => {
                                this.saveClicked=false;
                                this._changeDetectorRef.markForCheck();
                            })
                        )
                        .subscribe((response)=>{
                            if (!response?.roleUtilisateurId){
                                return;
                            }

                            setTimeout(()=> {
                                fromPromise(this._router.navigate(['../'], { relativeTo: this._activatedRoute, replaceUrl: true }))
                                    .subscribe();
                            }, 200);

                        });

                    return;
                }

                this._roleUtilisateurService
                    .UpdateRoleUtilisateur(roleUtilisateur)
                    .pipe(
                        finalize(() => {
                            this.saveClicked=false;
                            this._changeDetectorRef.markForCheck();
                        })
                    )
                    .subscribe((response)=>{
                        if (!response){
                            return;
                        }

                        setTimeout(()=> {
                            fromPromise(this._router.navigate(['../'], { relativeTo: this._activatedRoute, replaceUrl: true }))
                                .subscribe();
                        }, 200);

                    });

            });
    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    protected readonly FuseNavigationAction = FuseNavigationAction;
}
