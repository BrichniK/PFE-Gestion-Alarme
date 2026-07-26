import { ResolveFn } from '@angular/router';
import { PagedDevice } from '../../../core/device/device.model';
import { inject } from '@angular/core';
import { DeviceService } from '../../../core/device/device.service';

export const deviceResolver: ResolveFn<PagedDevice> = (route, state) => {
    return inject(DeviceService).GetDevice();
};
