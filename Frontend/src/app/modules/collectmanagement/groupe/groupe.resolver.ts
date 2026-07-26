import { ResolveFn } from '@angular/router';
import { PagedGroupe } from '../../../core/groupe/groupe.model';
import { inject } from '@angular/core';
import { GroupeService } from '../../../core/groupe/groupe.service';

export const groupeResolver: ResolveFn<PagedGroupe> = (route, state) => {
    return inject(GroupeService).GetGroupe();
};
