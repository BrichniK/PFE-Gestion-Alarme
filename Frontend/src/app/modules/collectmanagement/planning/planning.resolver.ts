import { ResolveFn } from '@angular/router';
import { PagedPlanning } from '../../../core/planning/planning.model';
import { inject } from '@angular/core';
import { PlanningService } from '../../../core/planning/planning.service';

export const planningResolver: ResolveFn<PagedPlanning> = (route, state) => {
    return inject(PlanningService).GetPlanning(1, 10000);
};
