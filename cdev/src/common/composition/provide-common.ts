import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { DialogModule } from '@angular/cdk/dialog';
import { ErrorHandler, importProvidersFrom, inject, makeEnvironmentProviders, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserClock, KeycloakAuthClient, MockEndpointLogger } from '@cdev/common/infrastructure';
import { CdkConfirmationDialog, GlobalErrorHandler, GlobalErrorState } from '@cdev/common/presentation';
import { AUTH_CLIENT } from './auth/auth-client.token';
import { CONFIRMATION_DIALOG } from './confirmations/confirmation-dialog.token';
import { ERROR_REPORTER } from './errors/error-reporter.token';
import { authTokenInterceptor } from './http/auth-token.interceptor';
import { httpErrorInterceptor } from './http/http-error.interceptor';

export function provideCommonModule() {
  return makeEnvironmentProviders([
    importProvidersFrom(DialogModule),
    provideHttpClient(withInterceptors([authTokenInterceptor, httpErrorInterceptor])),
    { provide: AUTH_CLIENT, useFactory: () => new KeycloakAuthClient() },
    provideAppInitializer(() => inject(AUTH_CLIENT).initialize()),
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    GlobalErrorState,
    { provide: ERROR_REPORTER, useExisting: GlobalErrorState },
    { provide: CONFIRMATION_DIALOG, useClass: CdkConfirmationDialog },
    BrowserClock,
    MockEndpointLogger,
  ]);
}
