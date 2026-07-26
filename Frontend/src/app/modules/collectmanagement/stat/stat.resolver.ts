import { ActivatedRouteSnapshot, ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { StatService } from '../../../core/stat/stat.service';
import { MaintenanceStatResponse } from '../../../core/stat/stat.model';

export const statResolver: ResolveFn<MaintenanceStatResponse> = (
    route: ActivatedRouteSnapshot
) => {
    return inject(StatService).GetStats();
};
