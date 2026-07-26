/* eslint-disable */
import { FuseNavigationAction, FuseNavigationItem } from '@fuse/components/navigation';

export const defaultNavigation: FuseNavigationItem[] = [
    // Dashboard
    {
        id: 'home',
        title: 'Tableau de Bord',
        type: 'basic',
        icon: 'heroicons_outline:chart-bar',
        link: '/',
        exactMatch: true,
    },

    // Visaulization
    {
        id: 'fichier.visaulization',
        title: 'Visaulization',
        type: 'basic',
        icon: 'heroicons_outline:squares-2x2',
        link: '/fichier/visaulization',
    },

    // Monitoring
    {
        id: 'fichier.monitoring',
        title: 'KPI',
        type: 'basic',
        icon: 'heroicons_outline:chart-pie',
        link: '/fichier/monitoring',
    },

    // Ressources Humaines
    {
        id: 'ressources-humaines',
        title: 'Personnel',
        type: 'collapsable',
        icon: 'heroicons_outline:user-group',
        children: [
            {
                id: 'fichier.employee',
                title: 'Employés',
                type: 'basic',
                icon: 'mat_outline:badge',
                link: '/fichier/employee',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.shift',
                title: 'Shifts',
                type: 'basic',
                icon: 'mat_outline:schedule',
                link: '/fichier/shift',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.planning',
                title: 'Planning',
                type: 'basic',
                icon: 'mat_outline:calendar_today',
                link: '/fichier/planning',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
        ],
    },

    // Équipements
    {
        id: 'equipements',
        title: 'GOM',
        type: 'collapsable',
        icon: 'heroicons_outline:device-tablet',
        children: [
            {
                id: 'fichier.alerte',
                title: 'Ordre De Maintenance',
                type: 'basic',
                icon: 'mat_outline:notifications_active',
                link: '/fichier/alerte',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.maintenance',
                title: 'Suivi Ordre de maintenance',
                type: 'basic',
                icon: 'mat_outline:build',
                link: '/fichier/maintenance',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
        ],
    },

    // Administration
    {
        id: 'administration',
        title: 'Configuration',
        type: 'collapsable',
        icon: 'heroicons_outline:cog-6-tooth',
        children: [
            {
                id: 'fichier.societe',
                title: 'Société',
                type: 'basic',
                icon: 'mat_outline:business',
                link: '/fichier/societe',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.utilisateur',
                title: 'Utilisateurs',
                type: 'basic',
                icon: 'mat_outline:group',
                link: '/fichier/utilisateur',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.role-utilisateur',
                title: 'Rôles & Permissions',
                type: 'basic',
                icon: 'mat_outline:manage_accounts',
                link: '/fichier/role-utilisateur',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.jour-ferie',
                title: 'Jours Fériés',
                type: 'basic',
                icon: 'mat_outline:celebration',
                link: '/fichier/jour-ferie',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'administration.sms',
                title: 'SMS',
                type: 'basic',
                icon: 'mat_outline:sms',
                link: '/fichier/sms',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'administration.sms-configuration',
                title: 'Configuration SMS',
                type: 'basic',
                icon: 'mat_outline:settings',
                link: '/fichier/sms-configuration',
                action: [FuseNavigationAction.Edit],
            },
            {
                id: 'administration.configuration-generale',
                title: 'Configuration Générale',
                type: 'basic',
                icon: 'heroicons_outline:cog-6-tooth',
                link: '/fichier/configuration-generale',
                action: [FuseNavigationAction.Edit],
            },
            {
                id: 'fichier.device',
                title: 'Appareils',
                type: 'basic',
                icon: 'heroicons_outline:computer-desktop',
                link: '/fichier/device',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.type',
                title: "Type d'alerte",
                type: 'basic',
                icon: 'mat_outline:category',
                link: '/fichier/type',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'fichier.groupe',
                title: 'Groupes',
                type: 'basic',
                icon: 'mat_outline:groups',
                link: '/fichier/groupe',
                action: [
                    FuseNavigationAction.Add,
                    FuseNavigationAction.Edit,
                    FuseNavigationAction.Delete,
                ],
            },
            {
                id: 'administration.reset',
                title: 'Reset',
                type: 'basic',
                icon: 'heroicons_outline:arrow-path',
                link: '/fichier/reset',
            },
        ],
    },

    // Reporting
    {
        id: 'reporting',
        title: 'Reporting',
        type: 'collapsable',
        icon: 'heroicons_outline:chart-bar-square',
        children: [
            {
                id: 'reporting.stat',
                title: 'Statistiques',
                type: 'basic',
                icon: 'mat_outline:analytics',
                link: '/reporting/stat',
            },
            {
                id: 'reporting.diagramme-gantt',
                title: 'diagramme gantt',
                type: 'basic',
                icon: 'mat_outline:analytics',
                link: '/reporting/diagramme-gantt',
            },
        ],
    },

    // KPI
    // {
    //     id: 'kpi',
    //     title: 'KPI',
    //     type: 'basic',
    //     icon: 'mat_outline:speed',
    //     link: '/reporting/kpi',
    // },
];

export const compactNavigation: FuseNavigationItem[] = [
    {
        id: 'home',
        tooltip: 'Tableau de Bord',
        title: 'Tableau de Bord',
        type: 'basic',
        icon: 'heroicons_outline:chart-bar',
        link: '/',
        exactMatch: true,
    },

    {
        id: 'fichier.visaulization',
        tooltip: 'Visaulization',
        title: 'Visaulization',
        type: 'basic',
        icon: 'heroicons_outline:squares-2x2',
        link: '/fichier/visaulization',
    },

    {
        id: 'fichier.monitoring',
        tooltip: 'Monitoring',
        title: 'Monitoring',
        type: 'basic',
        icon: 'heroicons_outline:chart-pie',
        link: '/fichier/monitoring',
    },

    {
        id: 'ressources-humaines',
        title: 'Personnel',
        tooltip: 'Personnel',
        type: 'aside',
        icon: 'heroicons_outline:user-group',
        children: []
    },

    {
        id: 'equipements',
        title: 'Équipements',
        tooltip: 'Équipements',
        type: 'aside',
        icon: 'heroicons_outline:device-tablet',
        children: []
    },

    {
        id: 'administration',
        title: 'Administration',
        tooltip: 'Administration',
        type: 'aside',
        icon: 'heroicons_outline:cog-6-tooth',
        children: []
    },

    {
        id: 'reporting',
        title: 'Reporting',
        tooltip: 'Reporting',
        type: 'aside',
        icon: 'heroicons_outline:chart-bar-square',
        children: [
            {
                id: 'reporting.stat',
                title: 'Statistiques',
                type: 'basic',
                icon: 'mat_outline:analytics',
                link: '/reporting/stat',
            },
        ]
    },

    // KPI
    // {
    //     id: 'kpi',
    //     tooltip: 'KPI',
    //     title: 'KPI',
    //     type: 'basic',
    //     icon: 'mat_outline:speed',
    //     link: '/reporting/kpi',
    // },
];

export const futuristicNavigation: FuseNavigationItem[] = [

    {
        id: 'homePage',
        title: 'Home Page',
        type: 'group',
        children: []
    },
    {
        id: 'gestionOperation',
        title: 'Analysis',
        type: 'group',
        children: []
    },
    {
        id: 'traceabilitys',
        title: 'Traceability',
        type: 'group',
        children: []
    },
    {
        id: 'gestion-service',
        title: 'Service management',
        type: 'group',
        children: []
    },

    {
        id: 'donnees',
        title: 'Satellite',
        type: 'group',
        children: []
    }, {
        id: 'gestion-utilisateur',
        title: 'User Management',
        type: 'group',
        children: []
    },
    {
        id: 'planning.calendar',
        title: 'Planning',
        type: 'group',
        children: []
    },
];

export const horizontalNavigation: FuseNavigationItem[] = [

    {
        id: 'fichier',
        title: 'Fichier',
        type: 'group',
        icon: 'mat_outline:assignment',
        children: []
    },
    {
        id: 'planning.calendar',
        title: 'Planning',
        type: 'group',
        icon: 'mat_outline:calendar_month',
        children: []
    }



];
