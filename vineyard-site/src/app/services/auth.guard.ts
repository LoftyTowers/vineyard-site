import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = route.data?.['roles'] as string[] | undefined;

  if (!auth.token) {
    router.navigate(['/']);
    return false;
  }

  if (roles && roles.length && !roles.some(r => auth.hasRole(r))) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
