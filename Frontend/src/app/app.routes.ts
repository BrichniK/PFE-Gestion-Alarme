import { Route } from '@angular/router';
import { initialDataResolver } from 'app/app.resolvers';
import { AuthGuard } from 'app/core/auth/guards/auth.guard';
import { NoAuthGuard } from 'app/core/auth/guards/noAuth.guard';
import { LayoutComponent } from 'app/layout/layout.component';
import { navigationGuard } from './core/navigation/guards/navigation.guard';

// @formatter:off
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/explicit-function-return-type */
export const appRoutes: Route[] = [

    // Redirect empty path to '/example'
    { path: '', pathMatch: 'full', redirectTo: 'Accueil/page' },

    // Redirect signed-in user to the '/example'
    //
    // After the user signs in, the sign-in page will redirect the user to the 'signed-in-redirect'
    // path. Below is another redirection for that path to redirect the user to the desired
    // location. This is a small convenience to keep all main routes together here on this file.
    { path: 'signed-in-redirect', pathMatch: 'full', redirectTo: 'Accueil/page' },

    // Auth routes for guests
    {
        path: '',
        canActivate: [NoAuthGuard],
        canActivateChild: [NoAuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            //{path: 'confirmation-required', loadChildren: () => import('app/modules/auth/confirmation-required/confirmation-required.routes')},
            //{path: 'forgot-password', loadChildren: () => import('app/modules/auth/forgot-password/forgot-password.routes')},
            //{path: 'reset-password', loadChildren: () => import('app/modules/auth/reset-password/reset-password.routes')},
            { path: 'sign-in', loadChildren: () => import('app/modules/auth/sign-in/sign-in.routes') },
            //{path: 'sign-up', loadChildren: () => import('app/modules/auth/sign-up/sign-up.routes')}
        ]
    },

    // Auth routes for authenticated users
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'sign-out', loadChildren: () => import('app/modules/auth/sign-out/sign-out.routes') },
            { path: 'unlock-session', loadChildren: () => import('app/modules/auth/unlock-session/unlock-session.routes') }
        ]
    },

    // Landing routes
    // {
    //     path: '',
    //     component: LayoutComponent,
    //     data: {
    //         layout: 'empty'
    //     },
    //     children: [
    //         {path: 'home', loadChildren: () => import('app/modules/landing/home/home.routes')},
    //     ]
    // },

    // Admin routes
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        resolve: {
            initialData: initialDataResolver
        },
        children: [
            { path: 'example', loadChildren: () => import('app/modules/admin/example/example.routes') },
        ]
    },

    //Collect Management
    {
        path: '',
        canMatch: [AuthGuard],
        canActivateChild: [navigationGuard],
        component: LayoutComponent,
        resolve: {
            initialData: initialDataResolver,
        },
        children: [


            //Accueil
            {
                path: 'Accueil', children: [
                    { path: 'page', loadChildren: () => import('app/modules/collectmanagement/accueil/accueil.routes') },

                ]
            },
            //Fichier

            {
                path: 'fichier', children: [
                    {
                        path: 'utilisateur',
                        data: { navigationId: 'fichier.utilisateur' },
                        loadChildren: () => import('app/modules/collectmanagement/gestion-utilisateur/utilisateur/utilisateur.routes')
                    },
                    {
                        path: 'societe',
                        data: { navigationId: 'fichier.societe' },
                        loadChildren: () => import('./modules/cst/societe/societe.routes')
                    },
                    {
                        path: 'role-utilisateur',
                        data: { navigationId: 'fichier.role-utilisateur' },
                        loadChildren: () => import('app/modules/collectmanagement/gestion-utilisateur/role-utilisateur/role-utilisateur.routes')
                    },
                    {
                        path: 'sms',
                        data: { navigationId: 'administration.sms' },
                        loadChildren: () => import('app/modules/collectmanagement/sms/sms.routes')
                    },
                    {
                        path: 'employee',
                        data: { navigationId: 'fichier.employee' },
                        loadChildren: () => import('app/modules/collectmanagement/employee/employee.routes')
                    },
                    {
                        path: 'shift',
                        data: { navigationId: 'fichier.shift' },
                        loadChildren: () => import('app/modules/collectmanagement/shift/shift.routes')
                    },
                    {
                        path: 'device',
                        data: { navigationId: 'fichier.device' },
                        loadChildren: () => import('app/modules/collectmanagement/device/device.routes')
                    },
                    {path:'visaulization',
                        data:{navigationId: 'fichier.device'},
                        loadChildren:() => import('app/modules/collectmanagement/visaulization/visaulization.routes')},

                    {
                        path: 'monitoring',
                        data: { navigationId: 'fichier.monitoring' },
                        loadChildren: () => import('app/modules/collectmanagement/monitoring/monitoring.routes')
                    },

                    {
                        path: 'planning',
                        data: { navigationId: 'fichier.planning' },
                        loadChildren: () => import('app/modules/collectmanagement/planning/planning.routes')
                    },
                    {
                        path: 'maintenance',
                        data: { navigationId: 'fichier.maintenance' },
                        loadChildren: () => import('app/modules/collectmanagement/maintenance/maintenance.routes')
                    },
                    {
                        path: 'type',
                        data: { navigationId: 'fichier.type' },
                        loadChildren: () => import('app/modules/collectmanagement/type/type.routes')
                    },
                    {
                        path: 'alerte',
                        data: { navigationId: 'fichier.alerte' },
                        loadChildren: () => import('app/modules/collectmanagement/alerte/alerte.routes')
                    },
                    {
                        path: 'jour-ferie',
                        data: { navigationId: 'fichier.jour-ferie' },
                        loadChildren: () => import('app/modules/collectmanagement/jour-ferie/jour-ferie.routes')
                    },
                    {
                        path: 'sms-configuration',
                        data: { navigationId: 'administration.sms-configuration' },
                        loadChildren: () => import('app/modules/collectmanagement/sms-configuration/sms-configuration.routes')
                    },
                    {
                        path: 'configuration-generale',
                        data: { navigationId: 'administration.configuration-generale' },
                        loadChildren: () => import('app/modules/collectmanagement/configuration-generale/configuration-generale.routes')
                    },
                    {
                        path: 'groupe',
                        data: { navigationId: 'fichier.groupe' },
                        loadChildren: () => import('app/modules/collectmanagement/groupe/groupe.routes')
                    },
                    {
                        path: 'reset',
                        data: { navigationId: 'administration.reset' },
                        loadChildren: () => import('app/modules/collectmanagement/reset/reset.routes')
                    },
                ]
            },

            //Reporting
            {
                path: 'reporting', children: [
                    {
                        path: 'stat',
                        data: { navigationId: 'reporting.stat' },
                        loadChildren: () => import('app/modules/collectmanagement/stat/stat.routes')
                    },
                    {
                             path: 'kpi',
                        data: { navigationId: 'reporting.kpi' },
                        loadChildren: () => import('app/modules/collectmanagement/kpi/kpi.routes')
                    },
                    {
                        path: 'diagramme-gantt',
                        data: { navigationId: 'reporting.diagramme-gantt' },
                        loadChildren: () => import('app/modules/collectmanagement/diagramme-gantt/diagramme-gantt.routes')
                    },
                ]
            },









        ],

    }
];
