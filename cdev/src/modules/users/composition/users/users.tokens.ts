import { InjectionToken } from '@angular/core';
import { GetCurrentUserQueryHandler, RegisterUserCommandHandler, UserRepository } from '@cdev/modules/users/application';

export const USER_REPOSITORY = new InjectionToken<UserRepository>('USER_REPOSITORY');
export const GET_CURRENT_USER_HANDLER = new InjectionToken<GetCurrentUserQueryHandler>('GET_CURRENT_USER_HANDLER');
export const REGISTER_USER_HANDLER = new InjectionToken<RegisterUserCommandHandler>('REGISTER_USER_HANDLER');
