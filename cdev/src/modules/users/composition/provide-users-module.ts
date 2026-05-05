import { makeEnvironmentProviders } from '@angular/core';
import { MockEndpointLogger } from '@cdev/common/infrastructure';
import { GetCurrentUserQueryHandler, RegisterUserCommandHandler, UserRepository } from '@cdev/modules/users/application';
import { LocalUserRepository, MockUsersEndpoint } from '@cdev/modules/users/infrastructure';
import { UserSummaryFacade } from '@cdev/modules/users/presentation';
import { GET_CURRENT_USER_HANDLER, REGISTER_USER_HANDLER, USER_REPOSITORY } from './users.tokens';

export function provideUsersModule() {
  return makeEnvironmentProviders([
    {
      provide: MockUsersEndpoint,
      useFactory: (logger: MockEndpointLogger) => new MockUsersEndpoint(logger),
      deps: [MockEndpointLogger],
    },
    {
      provide: LocalUserRepository,
      useFactory: (endpoint: MockUsersEndpoint) => new LocalUserRepository(endpoint),
      deps: [MockUsersEndpoint],
    },
    UserSummaryFacade,
    { provide: USER_REPOSITORY, useExisting: LocalUserRepository },
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
