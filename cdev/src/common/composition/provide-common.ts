import { DialogModule } from '@angular/cdk/dialog';
import { ErrorHandler, importProvidersFrom, makeEnvironmentProviders, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserClock, MockEndpointLogger } from '@cdev/common/infrastructure';
import { CdkConfirmationDialog, GlobalErrorHandler, GlobalErrorState } from '@cdev/common/presentation';
import { CONFIRMATION_DIALOG } from './confirmations/confirmation-dialog.token';
import { ERROR_REPORTER } from './errors/error-reporter.token';

export function provideCommonModule() {
  return makeEnvironmentProviders([
    importProvidersFrom(DialogModule),
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    GlobalErrorState,
    { provide: ERROR_REPORTER, useExisting: GlobalErrorState },
    { provide: CONFIRMATION_DIALOG, useClass: CdkConfirmationDialog },
    BrowserClock,
    MockEndpointLogger,
  ]);
}
