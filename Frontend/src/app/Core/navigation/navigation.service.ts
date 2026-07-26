import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Navigation } from 'app/core/navigation/navigation.types';
import { map, Observable, of, ReplaySubject, switchMap, take, tap } from 'rxjs';
import {
    compactNavigation,
    defaultNavigation,
    futuristicNavigation,
    horizontalNavigation,
} from './navigation.data';
import { FuseNavigationItem, FuseNavigationService } from '../../../@fuse/components/navigation';
import { cloneDeep } from 'lodash-es';
import { UserService } from '../user/user.service';
import { RoleNavigation } from '../role-utilisateur/role-utilisateur.model';

@Injectable({ providedIn: 'root' })
export class NavigationService {
    private _userService = inject(UserService);
    private _fuseNavigationService = inject(FuseNavigationService);
    private _navigation: ReplaySubject<Navigation> =
        new ReplaySubject<Navigation>(1);

    private readonly _compactNavigation: FuseNavigationItem[] =
        compactNavigation;
    private readonly _defaultNavigation: FuseNavigationItem[] =
        defaultNavigation;
    private readonly _futuristicNavigation: FuseNavigationItem[] =
        futuristicNavigation;
    private readonly _horizontalNavigation: FuseNavigationItem[] =
        horizontalNavigation;

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for navigation
     */
    get navigation$(): Observable<Navigation> {
        return this._navigation.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get all navigation data
     */
    get(): Observable<Navigation> {

        return this._userService.user$
            .pipe(
                take(1),
                switchMap((user) => {

                    const flatNavigation = this._fuseNavigationService.getFlatNavigation(this._defaultNavigation);

                    const setB = flatNavigation.map(item => item.id);
                    const setA = user?.navigations?.map(item => item.navigationId)??[];
                    const permissionAliases: Record<string, string[]> = {
                        'fichier.visaulization': ['fichier.device'],
                        'fichier.monitoring': ['fichier.device'],
                        'administration.reset': ['fichier.device'],
                    };

                    let defaultNavigation: FuseNavigationItem[]=this._defaultNavigation;
                    let compactNavigation: FuseNavigationItem[]=this._compactNavigation;
                    let futuristicNavigation: FuseNavigationItem[]=this._futuristicNavigation;
                    let horizontalNavigation: FuseNavigationItem[]=this._horizontalNavigation;

                    const hasAccess = (navigationId: string): boolean => {
                        if (setA.length === 0){
                            return true;
                        }

                        // Always visible items
                        const alwaysVisible = ['home', 'fichier.monitoring'];
                        if (alwaysVisible.includes(navigationId)) {
                            return true;
                        }

                        if (setA.includes(navigationId)){
                            return true;
                        }

                        const aliases = permissionAliases[navigationId] ?? [];
                        return aliases.some(alias => setA.includes(alias));
                    };

                    setB?.forEach(item => {
                        if (!hasAccess(item)){
                            defaultNavigation = this._fuseNavigationService.removeItemById(defaultNavigation, item);
                            compactNavigation = this._fuseNavigationService.removeItemById(compactNavigation, item);
                            futuristicNavigation = this._fuseNavigationService.removeItemById(futuristicNavigation, item);
                            horizontalNavigation = this._fuseNavigationService.removeItemById(horizontalNavigation, item);
                        }
                    });

                    compactNavigation.forEach((compactNavItem) => {
                        defaultNavigation.forEach((defaultNavItem) => {
                            if (defaultNavItem.id === compactNavItem.id) {
                                compactNavItem.children = cloneDeep(
                                    defaultNavItem.children
                                );
                                compactNavItem.action = cloneDeep(
                                    defaultNavItem.action??[]
                                );
                                compactNavItem.section = cloneDeep(
                                    defaultNavItem.section??[]
                                );
                            }
                        });
                    });

                    // Fill futuristic navigation children using the default navigation
                    futuristicNavigation.forEach((futuristicNavItem) => {
                        defaultNavigation.forEach((defaultNavItem) => {
                            if (defaultNavItem.id === futuristicNavItem.id) {
                                futuristicNavItem.children = cloneDeep(
                                    defaultNavItem.children
                                );
                                futuristicNavItem.action = cloneDeep(
                                    defaultNavItem.action??[]
                                );
                                futuristicNavItem.section = cloneDeep(
                                    defaultNavItem.section??[]
                                );
                            }
                        });
                    });

                    // Fill horizontal navigation children using the default navigation
                    horizontalNavigation.forEach((horizontalNavItem) => {
                        defaultNavigation.forEach((defaultNavItem) => {
                            if (defaultNavItem.id === horizontalNavItem.id) {
                                horizontalNavItem.children = cloneDeep(
                                    defaultNavItem.children
                                );
                                horizontalNavItem.action = cloneDeep(
                                    defaultNavItem.action??[]
                                );
                                horizontalNavItem.section = cloneDeep(
                                    defaultNavItem.section??[]
                                );
                            }
                        });
                    });

                    const navigation = {
                        compact: cloneDeep(compactNavigation),
                        default: cloneDeep(this._defaultNavigation),
                        futuristic: cloneDeep(futuristicNavigation),
                        horizontal: cloneDeep(horizontalNavigation),
                    }

                    this._navigation.next(navigation);

                    return of(navigation);
                }));
    }
}
