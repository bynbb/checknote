import { MockEndpointLogger } from '@cdev/common/infrastructure';
import { User } from '@cdev/modules/users/domain';

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
