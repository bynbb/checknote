import { HttpClient } from '@angular/common/http';
import { makeEnvironmentProviders } from '@angular/core';
import { GetCurrentUserQueryHandler, RegisterUserCommandHandler, UserRepository } from '@cdev/modules/users/application';
import { UserSummaryFacade } from '@cdev/modules/users/presentation';
import { AngularHttpUserRepository } from './angular-http-user-repository';
import { GET_CURRENT_USER_HANDLER, REGISTER_USER_HANDLER, USER_REPOSITORY } from './users.tokens';

export function provideUsersModule() {
  return makeEnvironmentProviders([
    {
      provide: AngularHttpUserRepository,
      useFactory: (http: HttpClient) => new AngularHttpUserRepository(http),
      deps: [HttpClient],
    },
    UserSummaryFacade,
    { provide: USER_REPOSITORY, useExisting: AngularHttpUserRepository },
    {
      provide: GET_CURRENT_USER_HANDLER,
      useFactory: (repository: UserRepository) => new GetCurrentUserQueryHandler(repository),
      deps: [USER_REPOSITORY],
    },
    {
      provide: REGISTER_USER_HANDLER,
      useFactory: (repository: UserRepository) => new RegisterUserCommandHandler(repository),
      deps: [USER_REPOSITORY],
    },
  ]);
}
