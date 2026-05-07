import { ErrorHandler, makeEnvironmentProviders, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserClock, MockEndpointLogger } from '@cdev/common/infrastructure';
import { GlobalErrorHandler, GlobalErrorState } from '@cdev/common/presentation';
import { ERROR_REPORTER } from './errors/error-reporter.token';

export function provideCommonModule() {
  return makeEnvironmentProviders([
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    GlobalErrorState,
    { provide: ERROR_REPORTER, useExisting: GlobalErrorState },
    BrowserClock,
    MockEndpointLogger,
  ]);
}
