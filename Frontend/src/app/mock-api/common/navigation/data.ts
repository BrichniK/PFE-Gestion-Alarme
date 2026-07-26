/* eslint-disable */
import { FuseNavigationAction, FuseNavigationItem } from '@fuse/components/navigation';

export const defaultNavigation: FuseNavigationItem[] = [

    {
        id: 'dashboard',
        title: 'Dashboard',
        type: 'basic',
        icon: 'heroicons_outline:squares-2x2',
        link: '/dashboard'
    },
    {
        id: 'alerts',
        title: 'Alerts',
        type: 'group',
        icon: 'heroicons_outline:bell',
        children: [
            {
                id: 'alerts.active',
                title: 'Active Alerts',
                type: 'basic',
                link: '/alerts/active',
                icon: 'heroicons_outline:exclamation-circle'
            },
            {
                id: 'alerts.history',
                title: 'History',
                type: 'basic',
                link: '/alerts/history',
                icon: 'heroicons_outline:clock'
            }
        ]
    },
    {
        id: 'management',
        title: 'Management',
        type: 'group',
        icon: 'heroicons_outline:user-group',
        children: [
            {
                id: 'management.employees',
                title: 'Employees',
                type: 'basic',
                link: '/employees',
                icon: 'heroicons_outline:users'
            },
            {
                id: 'management.devices',
                title: 'Devices',
                type: 'basic',
                link: '/devices',
                icon: 'heroicons_outline:device-tablet'
            }
        ]
    },
    {
        id: 'settings',
        title: 'Settings',
        type: 'basic',
        icon: 'heroicons_outline:cog-8-tooth',
        link: '/settings'
    }


];

export const compactNavigation: FuseNavigationItem[] = [
    {
        id: 'homePage',
        tooltip: 'Home Page',
        title: 'H.P',
        type: 'aside',
        icon: 'heroicons_outline:home',
        children: []
    },

    {
        id: 'analysis',
        title: 'A',
        tooltip: 'Analysis',
        type: 'aside',
        icon: 'mat_outline:analytics',
        children: []
    },

    {
        id: 'gestion-service',
        title: 'S.M',
        tooltip: 'Service management',
        type: 'aside',
        icon: 'heroicons_outline:user-group',
        children: []
    },

    {
        id: 'donnees',
        title: 'S',
        tooltip: 'Satellite',
        type: 'aside',
        icon: 'heroicons_outline:cog-8-tooth',
        children: []
    },
    {
        id: 'gestion-utilisateur',
        title: 'U.M',
        tooltip: 'User Management',
        type: 'aside',
        icon: 'heroicons_outline:user-plus',
        children: []
    },
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
        id: 'dashboards',
        title: 'Dashboard',
        type: 'group',
        icon: 'mat_outline:assignment',
        children: []
    },
    {
        id: 'bonMelanges',
        title: 'Bon De Mélange',
        type: 'group',
        icon: 'heroicons_outline:cog-8-tooth',
        children: []
    },
    {
        id: 'configurations',
        title: 'Configurations',
        type: 'group',
        icon: 'heroicons_outline:cog-8-tooth',
        children: []
    },


];
