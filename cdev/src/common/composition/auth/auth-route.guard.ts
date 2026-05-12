import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AUTH_CLIENT } from './auth-client.token';

export const authenticatedRouteGuard: CanActivateFn = () => {
  const auth = inject(AUTH_CLIENT);

  if (auth.getState().status === 'authenticated') {
    return true;
  }

  return inject(Router).createUrlTree(['/sign-in']);
};
