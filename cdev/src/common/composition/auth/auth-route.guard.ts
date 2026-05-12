import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AUTH_CLIENT } from './auth-client.token';

export const authenticatedRouteGuard: CanActivateFn = () => {
  const auth = inject(AUTH_CLIENT);
  const state = auth.getState();

  if (state.status === 'authenticated') {
    return true;
  }

  if (state.status === 'unavailable') {
    return inject(Router).createUrlTree(['/sign-in']);
  }

  void auth.login().catch(() => undefined);
  return false;
};

export const signInRouteGuard: CanActivateFn = () => {
  const auth = inject(AUTH_CLIENT);
  const state = auth.getState();

  if (state.status === 'authenticated') {
    return inject(Router).createUrlTree(['/']);
  }

  if (state.status === 'unavailable') {
    return true;
  }

  void auth.login().catch(() => undefined);
  return false;
};
