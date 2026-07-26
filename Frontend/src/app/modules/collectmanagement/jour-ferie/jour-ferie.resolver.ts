import { ResolveFn } from '@angular/router';
import { PagedJourFerie } from '../../../core/jour-ferie/jour-ferie.model';
import { inject } from '@angular/core';
import { JourFerieService } from '../../../core/jour-ferie/jour-ferie.service';

export const jourFerieResolver: ResolveFn<PagedJourFerie> = () => {
    return inject(JourFerieService).GetJourFerie();
};
