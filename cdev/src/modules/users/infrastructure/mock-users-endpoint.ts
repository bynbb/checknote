import { MockEndpointLogger } from '../../../common/infrastructure/mock-endpoint-logger';
import { User } from '../domain/user';

export class MockUsersEndpoint {
  constructor(private readonly logger: MockEndpointLogger) {}

  getCurrentUser(): User {
    this.logger.log('GET /api/users/me');
    return {
      id: 'user-1',
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    };
  }

  registerUser(name: string, email: string): User {
    this.logger.log('POST /api/users');
    return {
      id: `user-${Date.now()}`,
      name,
      email,
    };
  }
}
