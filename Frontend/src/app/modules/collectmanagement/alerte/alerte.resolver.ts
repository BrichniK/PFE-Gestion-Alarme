import { ResolveFn } from '@angular/router';
import { PagedAlerte } from '../../../core/alerte/alerte.model';
import { inject } from '@angular/core';
import { AlerteService } from '../../../core/alerte/alerte.service';

export const alerteResolver: ResolveFn<PagedAlerte> = (route, state) => {
    return inject(AlerteService).GetAlerte();
};
