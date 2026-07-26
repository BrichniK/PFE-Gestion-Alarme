import { ResolveFn } from '@angular/router';
import { PagedType } from '../../../core/type/type.model';
import { inject } from '@angular/core';
import { TypeService } from '../../../core/type/type.service';

export const typeResolver: ResolveFn<PagedType> = (route, state) => {
    return inject(TypeService).GetType();
};
