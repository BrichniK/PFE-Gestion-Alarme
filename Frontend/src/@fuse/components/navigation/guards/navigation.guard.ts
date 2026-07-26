import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { firstValueFrom } from 'rxjs';
import { UserService } from '../../../../app/core/user/user.service';

export const navigationGuard: CanActivateFn | CanActivateChildFn = async (route, state) => {

    if (!route.data?.navigationId) {
        return true;
    }

    const userService = inject(UserService);
    const router: Router = inject(Router);
    const user = await firstValueFrom(userService.user$);

    if (user?.navigations.length === 0){
        return true;
    }

    const navigationIndex = user?.navigations
        ?.findIndex(n => n.navigationId===route.data?.navigationId);

    if (navigationIndex === -1){
        return router.navigate(['/accueil']);
    }

    return true;
}
