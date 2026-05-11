import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { createApplicationErrorFromHttpResponse, isChecknoteApiRequest } from '@cdev/common/infrastructure';
import { catchError, throwError } from 'rxjs';

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => next(request).pipe(
  catchError((error: unknown) => {
    if (error instanceof HttpErrorResponse && isChecknoteApiRequest(request.url)) {
      return throwError(() => createApplicationErrorFromHttpResponse(error.status, error.error));
    }

    return throwError(() => error);
  }),
);
