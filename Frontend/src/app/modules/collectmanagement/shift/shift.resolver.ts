import { ResolveFn } from '@angular/router';
import { PagedShift } from '../../../core/shift/shift.model';
import { inject } from '@angular/core';
import { ShiftService } from '../../../core/shift/shift.service';

export const shiftResolver: ResolveFn<PagedShift> = (route, state) => {
    return inject(ShiftService).GetShift();
};
