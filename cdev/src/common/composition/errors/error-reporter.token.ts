import { InjectionToken } from '@angular/core';
import { ErrorReporter } from '@cdev/common/application';

export const ERROR_REPORTER = new InjectionToken<ErrorReporter>('ERROR_REPORTER');
