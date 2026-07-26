import { NgTemplateOutlet } from '@angular/common';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import {
    FuseNavigationService,
    FuseVerticalNavigationComponent,
} from '@fuse/components/navigation';
import { AvailableLangs, TranslocoService } from '@ngneat/transloco';
import { take } from 'rxjs';

@Component({
    selector: 'languages',
    templateUrl: './languages.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    exportAs: 'languages',
    standalone: true,
    imports: [MatButtonModule, MatMenuModule, NgTemplateOutlet],
})
export class LanguagesComponent implements OnInit, OnDestroy {
    availableLangs: AvailableLangs;
    activeLang: string;
    flagCodes: any;

    /**
     * Constructor
     */
    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseNavigationService: FuseNavigationService,
        private _translocoService: TranslocoService
    ) {}

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        // Get the available languages from transloco
        this.availableLangs = this._translocoService.getAvailableLangs();

        // Subscribe to language changes
        this._translocoService.langChanges$.subscribe((activeLang) => {
            // Get the active lang
            this.activeLang = activeLang;

            // Update the navigation
            this._updateNavigation(activeLang);
        });

        // Set the country iso codes for languages for flags
        this.flagCodes = {
            en: 'us',
            tr: 'tr',
            fr:'fr',
            es:'es',
            it:'it'
        };
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {}

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Set the active lang
     *
     * @param lang
     */
    setActiveLang(lang: string): void {
        // Set the active lang
        this._translocoService.setActiveLang(lang);
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Update the navigation
     *
     * @param lang
     * @private
     */
    private _updateNavigation(lang: string): void {
        const navComponent =
            this._fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(
                'mainNavigation'
            );

        if (!navComponent) {
            return null;
        }

        const navigation = navComponent.navigation;

        // Helper to update a single item's title
        const updateItem = (id: string, translationKey: string) => {
            const item = this._fuseNavigationService.getItem(id, navigation);
            if (item) {
                this._translocoService
                    .selectTranslate(translationKey)
                    .pipe(take(1))
                    .subscribe((translation) => {
                        item.title = translation;
                        navComponent.refresh();
                    });
            }
        };

        // Dashboard
        updateItem('home', 'Tableau de Bord');

        // Visaulization
        updateItem('fichier.visaulization', 'Visaulization');

        // KPI / Monitoring
        updateItem('fichier.monitoring', 'KPI');

        // Personnel group
        updateItem('ressources-humaines', 'Personnel');
        updateItem('fichier.employee', 'Employés');
        updateItem('fichier.shift', 'Shifts');
        updateItem('fichier.planning', 'Planning');

        // Équipements group
        updateItem('equipements', 'GOM');
        updateItem('fichier.alerte', 'Ordre De Maintenance');
        updateItem('fichier.maintenance', 'Suivi Ordre de maintenance');

        // Administration / Configuration group
        updateItem('administration', 'Configuration');
        updateItem('fichier.societe', 'Société');
        updateItem('fichier.utilisateur', 'Utilisateurs');
        updateItem('fichier.role-utilisateur', 'Rôles & Permissions');
        updateItem('fichier.jour-ferie', 'Jours Fériés');
        updateItem('administration.sms', 'SMS');
        updateItem('administration.sms-configuration', 'Configuration SMS');
        updateItem('fichier.device', 'Appareils');
        updateItem('fichier.type', "Type d'alerte");
        updateItem('administration.reset', 'Reset');

        // Reporting group
        updateItem('reporting', 'Reporting');
        updateItem('reporting.stat', 'Statistiques');
        updateItem('reporting.diagramme-gantt', 'diagramme gantt');
    }
}
