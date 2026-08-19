import { Route } from '@angular/router';
import { initialDataResolver } from 'app/app.resolvers';
import { AuthGuard } from 'app/core/auth/guards/auth.guard';
import { NoAuthGuard } from 'app/core/auth/guards/noAuth.guard';
import { LayoutComponent } from 'app/layout/layout.component';
import { navigationGuard } from './core/navigation/guards/navigation.guard';

// IMPORT DU COMPONENT IA
import { AnalyseIaComponent } from 'app/modules/collectmanagement/analyse-ia/analyse-ia.component';

// @formatter:off
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/explicit-function-return-type */

export const appRoutes: Route[] = [

    // Redirect empty path to '/example'
    {
        path: '',
        pathMatch: 'full',
        redirectTo: 'Accueil/page'
    },

    // Redirect signed-in user
    {
        path: 'signed-in-redirect',
        pathMatch: 'full',
        redirectTo: 'Accueil/page'
    },

    // ============================================================
    // AUTH ROUTES FOR GUESTS
    // ============================================================
    {
        path: '',
        canActivate: [NoAuthGuard],
        canActivateChild: [NoAuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            {
                path: 'sign-in',
                loadChildren: () =>
                    import('app/modules/auth/sign-in/sign-in.routes')
            }
        ]
    },

    // ============================================================
    // AUTH ROUTES FOR AUTHENTICATED USERS
    // ============================================================
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            {
                path: 'sign-out',
                loadChildren: () =>
                    import('app/modules/auth/sign-out/sign-out.routes')
            },
            {
                path: 'unlock-session',
                loadChildren: () =>
                    import('app/modules/auth/unlock-session/unlock-session.routes')
            }
        ]
    },

    // ============================================================
    // ADMIN ROUTES
    // ============================================================
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        resolve: {
            initialData: initialDataResolver
        },
        children: [
            {
                path: 'example',
                loadChildren: () =>
                    import('app/modules/admin/example/example.routes')
            }
        ]
    },

    // ============================================================
    // COLLECT MANAGEMENT
    // ============================================================
    {
        path: '',
        canMatch: [AuthGuard],
        canActivateChild: [navigationGuard],
        component: LayoutComponent,
        resolve: {
            initialData: initialDataResolver
        },
        children: [

            // ====================================================
            // ACCUEIL
            // ====================================================
            {
                path: 'Accueil',
                children: [
                    {
                        path: 'page',
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/accueil/accueil.routes'
                            )
                    }
                ]
            },

            // ====================================================
            // FICHIER
            // ====================================================
            {
                path: 'fichier',
                children: [

                    // ------------------------------------------------
                    // UTILISATEUR
                    // ------------------------------------------------
                    {
                        path: 'utilisateur',
                        data: {
                            navigationId: 'fichier.utilisateur'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/gestion-utilisateur/utilisateur/utilisateur.routes'
                            )
                    },

                    // ------------------------------------------------
                    // SOCIETE
                    // ------------------------------------------------
                    {
                        path: 'societe',
                        data: {
                            navigationId: 'fichier.societe'
                        },
                        loadChildren: () =>
                            import(
                                './modules/cst/societe/societe.routes'
                            )
                    },

                    // ------------------------------------------------
                    // ROLE UTILISATEUR
                    // ------------------------------------------------
                    {
                        path: 'role-utilisateur',
                        data: {
                            navigationId: 'fichier.role-utilisateur'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/gestion-utilisateur/role-utilisateur/role-utilisateur.routes'
                            )
                    },

                    // ------------------------------------------------
                    // SMS
                    // ------------------------------------------------
                    {
                        path: 'sms',
                        data: {
                            navigationId: 'administration.sms'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/sms/sms.routes'
                            )
                    },

                    // ------------------------------------------------
                    // EMPLOYEE
                    // ------------------------------------------------
                    {
                        path: 'employee',
                        data: {
                            navigationId: 'fichier.employee'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/employee/employee.routes'
                            )
                    },

                    // =================================================
                    // ANALYSE IA
                    // =================================================
{
    path: 'analyse-ia/:deviceId',
    data: {
        navigationId: 'fichier.device'
    },
    component: AnalyseIaComponent
},

                    // ------------------------------------------------
                    // SHIFT
                    // ------------------------------------------------
                    {
                        path: 'shift',
                        data: {
                            navigationId: 'fichier.shift'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/shift/shift.routes'
                            )
                    },

                    // ------------------------------------------------
                    // DEVICE
                    // ------------------------------------------------
                    {
                        path: 'device',
                        data: {
                            navigationId: 'fichier.device'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/device/device.routes'
                            )
                    },

                    // ------------------------------------------------
                    // VISUALIZATION
                    // ------------------------------------------------
                    {
                        path: 'visaulization',
                        data: {
                            navigationId: 'fichier.device'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/visaulization/visaulization.routes'
                            )
                    },

                    // ------------------------------------------------
                    // MONITORING
                    // ------------------------------------------------
                    {
                        path: 'monitoring',
                        data: {
                            navigationId: 'fichier.monitoring'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/monitoring/monitoring.routes'
                            )
                    },

                    // ------------------------------------------------
                    // PLANNING
                    // ------------------------------------------------
                    {
                        path: 'planning',
                        data: {
                            navigationId: 'fichier.planning'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/planning/planning.routes'
                            )
                    },

                    // ------------------------------------------------
                    // MAINTENANCE
                    // ------------------------------------------------
                    {
                        path: 'maintenance',
                        data: {
                            navigationId: 'fichier.maintenance'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/maintenance/maintenance.routes'
                            )
                    },

                    // ------------------------------------------------
                    // TYPE
                    // ------------------------------------------------
                    {
                        path: 'type',
                        data: {
                            navigationId: 'fichier.type'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/type/type.routes'
                            )
                    },

                    // ------------------------------------------------
                    // ALERTE
                    // ------------------------------------------------
                    {
                        path: 'alerte',
                        data: {
                            navigationId: 'fichier.alerte'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/alerte/alerte.routes'
                            )
                    },

                    // ------------------------------------------------
                    // JOUR FERIE
                    // ------------------------------------------------
                    {
                        path: 'jour-ferie',
                        data: {
                            navigationId: 'fichier.jour-ferie'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/jour-ferie/jour-ferie.routes'
                            )
                    },

                    // ------------------------------------------------
                    // SMS CONFIGURATION
                    // ------------------------------------------------
                    {
                        path: 'sms-configuration',
                        data: {
                            navigationId: 'administration.sms-configuration'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/sms-configuration/sms-configuration.routes'
                            )
                    },

                    // ------------------------------------------------
                    // CONFIGURATION GENERALE
                    // ------------------------------------------------
                    {
                        path: 'configuration-generale',
                        data: {
                            navigationId: 'administration.configuration-generale'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/configuration-generale/configuration-generale.routes'
                            )
                    },

                    // ------------------------------------------------
                    // GROUPE
                    // ------------------------------------------------
                    {
                        path: 'groupe',
                        data: {
                            navigationId: 'fichier.groupe'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/groupe/groupe.routes'
                            )
                    },

                    // ------------------------------------------------
                    // RESET
                    // ------------------------------------------------
                    {
                        path: 'reset',
                        data: {
                            navigationId: 'administration.reset'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/reset/reset.routes'
                            )
                    }
                ]
            },

            // ====================================================
            // REPORTING
            // ====================================================
            {
                path: 'reporting',
                children: [

                    // ------------------------------------------------
                    // STAT
                    // ------------------------------------------------
                    {
                        path: 'stat',
                        data: {
                            navigationId: 'reporting.stat'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/stat/stat.routes'
                            )
                    },

                    // ------------------------------------------------
                    // KPI
                    // ------------------------------------------------
                    {
                        path: 'kpi',
                        data: {
                            navigationId: 'reporting.kpi'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/kpi/kpi.routes'
                            )
                    },

                    // ------------------------------------------------
                    // DIAGRAMME GANTT
                    // ------------------------------------------------
                    {
                        path: 'diagramme-gantt',
                        data: {
                            navigationId: 'reporting.diagramme-gantt'
                        },
                        loadChildren: () =>
                            import(
                                'app/modules/collectmanagement/diagramme-gantt/diagramme-gantt.routes'
                            )
                    }
                ]
            }
        ]
    }
];