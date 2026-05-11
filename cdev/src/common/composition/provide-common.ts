import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { DialogModule } from '@angular/cdk/dialog';
import { ErrorHandler, importProvidersFrom, makeEnvironmentProviders, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserClock, MockEndpointLogger } from '@cdev/common/infrastructure';
import { CdkConfirmationDialog, GlobalErrorHandler, GlobalErrorState } from '@cdev/common/presentation';
import { CONFIRMATION_DIALOG } from './confirmations/confirmation-dialog.token';
import { ERROR_REPORTER } from './errors/error-reporter.token';
import { httpErrorInterceptor } from './http/http-error.interceptor';

export function provideCommonModule() {
  return makeEnvironmentProviders([
    importProvidersFrom(DialogModule),
    provideHttpClient(withInterceptors([httpErrorInterceptor])),
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    GlobalErrorState,
    { provide: ERROR_REPORTER, useExisting: GlobalErrorState },
    { provide: CONFIRMATION_DIALOG, useClass: CdkConfirmationDialog },
    BrowserClock,
    MockEndpointLogger,
  ]);
}
