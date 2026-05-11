import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { createApplicationErrorFromHttpResponse } from '@cdev/common/infrastructure';
import { catchError, throwError } from 'rxjs';

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => next(request).pipe(
  catchError((error: unknown) => {
    if (error instanceof HttpErrorResponse && isApiRequest(request.url)) {
      return throwError(() => createApplicationErrorFromHttpResponse(error.status, error.error));
    }

    return throwError(() => error);
  }),
);

function isApiRequest(url: string): boolean {
  return url === '/api' ||
    url.startsWith('/api/') ||
    /^https?:\/\/[^/]+\/api(?:\/|$)/i.test(url);
}
