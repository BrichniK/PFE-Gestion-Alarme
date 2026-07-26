import { ResolveFn } from '@angular/router';
import { PagedMaintenance } from '../../../core/maintenance/maintenance.model';
import { inject } from '@angular/core';
import { MaintenanceService } from '../../../core/maintenance/maintenance.service';

export const maintenanceResolver: ResolveFn<PagedMaintenance> = (route, state) => {
    return inject(MaintenanceService).GetMaintenance();
};
