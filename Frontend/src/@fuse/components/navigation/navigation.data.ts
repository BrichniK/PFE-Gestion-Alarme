/* eslint-disable */
import { FuseNavigationAction, FuseNavigationItem } from '@fuse/components/navigation';

export const defaultNavigation: FuseNavigationItem[] = [
    // ============================================
    // TABLEAU DE BORD
    // ============================================
    {
        id   : 'dashboard',
        title: 'Tableau de Bord',
        type : 'basic',
        icon : 'mat_outline:dashboard',
        link : '/dashboards/dashboard',
        action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
    },

    {
        id   : 'divider-1',
        type : 'divider'
    },

    // ============================================
    // GESTION DE PRODUCTION
    // ============================================
    {
        id   : 'production',
        title: 'Production',
        subtitle: 'Gestion des opérations',
        type : 'group',
        icon : 'mat_outline:precision_manufacturing',
        children: [
            {
                id   : 'production.bonMelange',
                title: 'Bon de Mélange',
                type : 'basic',
                icon : 'mat_outline:receipt',
                link : 'bonMelanges/bonMelange',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'production.gestionBonMelange',
                title: 'Gestion des Bons',
                type : 'basic',
                icon : 'mat_outline:inventory_2',
                link : 'bonMelanges/gestionBonMelange',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
        ]
    },

    // ============================================
    // GESTION DES RESSOURCES
    // ============================================
    {
        id   : 'ressources',
        title: 'Ressources',
        subtitle: 'Personnel & Équipements',
        type : 'group',
        icon : 'mat_outline:business_center',
        children: [
            {
                id   : 'ressources.employee',
                title: 'Employés',
                type : 'basic',
                icon : 'mat_outline:badge',
                link : '/collectmanagement/employee',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'ressources.shift',
                title: 'Shifts',
                type : 'basic',
                icon : 'mat_outline:schedule',
                link : '/collectmanagement/shift',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'ressources.planning',
                title: 'Planning',
                type : 'basic',
                icon : 'mat_outline:calendar_today',
                link : '/collectmanagement/planning',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'ressources.divider',
                type : 'divider'
            },
            {
                id   : 'ressources.device',
                title: 'Appareils',
                type : 'basic',
                icon : 'mat_outline:smartphone',
                link : '/collectmanagement/device',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'ressources.maintenance',
                title: 'Maintenance',
                type : 'basic',
                icon : 'mat_outline:build',
                link : '/collectmanagement/maintenance',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
        ]
    },

    {
        id   : 'divider-2',
        type : 'divider'
    },

    // ============================================
    // ADMINISTRATION
    // ============================================
    {
        id   : 'administration',
        title: 'Administration',
        subtitle: 'Paramètres système',
        type : 'group',
        icon : 'mat_outline:admin_panel_settings',
        children: [
            {
                id   : 'administration.societe',
                title: 'Société',
                type : 'basic',
                icon : 'mat_outline:business',
                link : '/fichier/societe',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'administration.utilisateur',
                title: 'Utilisateurs',
                type : 'basic',
                icon : 'mat_outline:group',
                link : '/fichier/utilisateur',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'administration.role',
                title: 'Rôles & Permissions',
                type : 'basic',
                icon : 'mat_outline:manage_accounts',
                link : '/fichier/role-utilisateur',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'administration.gestionUtilisateur',
                title: 'Gestion Utilisateurs',
                type : 'basic',
                icon : 'mat_outline:person_add',
                link : '/collectmanagement/gestion-utilisateur',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'administration.sms',
                title: 'SMS',
                type : 'basic',
                icon : 'mat_outline:sms',
                link : '/fichier/sms',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
        ]
    },

    // ============================================
    // CONFIGURATION TECHNIQUE
    // ============================================
    {
        id   : 'configuration',
        title: 'Configuration',
        subtitle: 'Paramètres techniques',
        type : 'group',
        icon : 'mat_outline:tune',
        children:[
            {
                id   : 'configuration.base',
                title: 'Configuration Base',
                type : 'basic',
                icon : 'mat_outline:developer_board',
                link : 'configurations/base',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
            {
                id   : 'configuration.formule',
                title: 'Formules',
                type : 'basic',
                icon : 'mat_outline:functions',
                link : 'configurations/formule',
                action:[FuseNavigationAction.Add, FuseNavigationAction.Edit, FuseNavigationAction.Delete]
            },
        ]
    },
];

export const compactNavigation: FuseNavigationItem[] = [
    {
        id   : 'homePage',
        tooltip: 'Home Page',
        title: 'H.P',
        type : 'aside',
        icon : 'heroicons_outline:home',
        children:[]
    },

    {
        id   : 'analysis',
        title: 'A',
        tooltip: 'Analysis',
        type : 'aside',
        icon : 'mat_outline:analytics',
        children: []
    },

    {
        id   : 'gestion-service',
        title: 'S.M',
        tooltip: 'Service management',
        type : 'aside',
        icon : 'heroicons_outline:user-group',
        children: []
    },

    {
        id   : 'donnees',
        title: 'S',
        tooltip: 'Satellite',
        type : 'aside',
        icon : 'heroicons_outline:cog-8-tooth',
        children: []
    },
    {
        id   : 'gestion-utilisateur',
        title: 'U.M',
        tooltip: 'User Management',
        type : 'aside',
        icon : 'heroicons_outline:user-plus',
        children: []
    },
];

export const futuristicNavigation: FuseNavigationItem[] = [

    {
        id   : 'homePage',
        title: 'Home Page',
        type : 'group',
        children:[]
    },
    {
        id   : 'gestionOperation',
        title: 'Analysis',
        type : 'group',
        children: []
    },
    {
        id   : 'traceabilitys',
        title: 'Traceability',
        type : 'group',
        children: []
    },
    {
        id   : 'gestion-service',
        title: 'Service management',
        type : 'group',
        children: []
    },

    {
        id   : 'donnees',
        title: 'Satellite',
        type : 'group',
        children: []
    },{
        id   : 'gestion-utilisateur',
        title: 'User Management',
        type : 'group',
        children: []
    },
];

export const horizontalNavigation: FuseNavigationItem[] = [
    {
        id   : 'dashboard',
        title: 'Tableau de Bord',
        type : 'basic',
        icon : 'mat_outline:dashboard',
        link : '/dashboards/dashboard',
    },
    {
        id   : 'production',
        title: 'Production',
        type : 'group',
        icon : 'mat_outline:precision_manufacturing',
        children: []
    },
    {
        id   : 'ressources',
        title: 'Ressources',
        type : 'group',
        icon : 'mat_outline:business_center',
        children: []
    },
    {
        id   : 'administration',
        title: 'Administration',
        type : 'group',
        icon : 'mat_outline:admin_panel_settings',
        children: []
    },
    {
        id   : 'configuration',
        title: 'Configuration',
        type : 'group',
        icon : 'mat_outline:tune',
        children: []
    },
];
