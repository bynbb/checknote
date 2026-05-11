import { InjectionToken } from '@angular/core';
import { AuthClient } from '@cdev/common/application';

export const AUTH_CLIENT = new InjectionToken<AuthClient>('AUTH_CLIENT');
