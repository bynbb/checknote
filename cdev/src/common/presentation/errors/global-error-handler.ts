import { ErrorHandler, Injectable, inject } from '@angular/core';
import { GlobalErrorState } from './global-error-state';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly errors = inject(GlobalErrorState);

  handleError(error: unknown): void {
    this.errors.report(error);
    console.error(GlobalErrorHandler.name, error);
  }
}
