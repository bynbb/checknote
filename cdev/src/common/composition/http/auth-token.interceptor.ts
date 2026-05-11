import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { isChecknoteApiRequest } from '@cdev/common/infrastructure';
import { from, switchMap } from 'rxjs';
import { AUTH_CLIENT } from '../auth/auth-client.token';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  if (!isChecknoteApiRequest(request.url)) {
    return next(request);
  }

  const authClient = inject(AUTH_CLIENT);

  return from(authClient.getAccessToken()).pipe(
    switchMap((token) => next(token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request)),
  );
};
