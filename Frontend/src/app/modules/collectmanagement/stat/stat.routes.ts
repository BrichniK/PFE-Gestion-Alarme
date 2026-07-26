import { StatComponent } from './stat.component';
import { statResolver } from './stat.resolver';
import { Routes } from '@angular/router';

export default [
    {
        path: '',
        component: StatComponent,
        resolve: {
            stats: statResolver,
        },
        title: 'Statistiques',
    },
] as Routes;
