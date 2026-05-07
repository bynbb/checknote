import { ErrorHandler, importProvidersFrom, makeEnvironmentProviders, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MatDialogModule } from '@angular/material/dialog';
import { BrowserClock, MockEndpointLogger } from '@cdev/common/infrastructure';
import { GlobalErrorHandler, GlobalErrorState, MaterialConfirmationDialog } from '@cdev/common/presentation';
import { CONFIRMATION_DIALOG } from './confirmations/confirmation-dialog.token';
import { ERROR_REPORTER } from './errors/error-reporter.token';

export function provideCommonModule() {
  return makeEnvironmentProviders([
    importProvidersFrom(MatDialogModule),
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    GlobalErrorState,
    { provide: ERROR_REPORTER, useExisting: GlobalErrorState },
    { provide: CONFIRMATION_DIALOG, useClass: MaterialConfirmationDialog },
    BrowserClock,
    MockEndpointLogger,
  ]);
}
