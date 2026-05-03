import { InjectionToken } from '@angular/core';
import { UserRepository } from '../application/abstractions/user-repository';
import { GetCurrentUserQueryHandler } from '../application/users/get-current-user/get-current-user.query-handler';
import { RegisterUserCommandHandler } from '../application/users/register-user/register-user.command-handler';

export const USER_REPOSITORY = new InjectionToken<UserRepository>('USER_REPOSITORY');
export const GET_CURRENT_USER_HANDLER = new InjectionToken<GetCurrentUserQueryHandler>('GET_CURRENT_USER_HANDLER');
export const REGISTER_USER_HANDLER = new InjectionToken<RegisterUserCommandHandler>('REGISTER_USER_HANDLER');
