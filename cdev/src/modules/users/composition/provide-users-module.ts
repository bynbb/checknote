import { makeEnvironmentProviders } from '@angular/core';
import { MockEndpointLogger } from '../../../common/infrastructure/mock-endpoint-logger';
import { UserRepository } from '../application/abstractions/user-repository';
import { GetCurrentUserQueryHandler } from '../application/users/get-current-user/get-current-user.query-handler';
import { RegisterUserCommandHandler } from '../application/users/register-user/register-user.command-handler';
import { LocalUserRepository } from '../infrastructure/local-user-repository';
import { MockUsersEndpoint } from '../infrastructure/mock-users-endpoint';
import { UserSummaryFacade } from '../presentation/user-summary.facade';
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
