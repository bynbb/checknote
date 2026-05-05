import { UserRepository } from '@cdev/modules/users/application';
import { User } from '@cdev/modules/users/domain';
import { MockUsersEndpoint } from './mock-users-endpoint';

export class LocalUserRepository implements UserRepository {
  constructor(private readonly endpoint: MockUsersEndpoint) {}

  async getCurrentUser(): Promise<User> {
    return this.endpoint.getCurrentUser();
  }

  async register(name: string, email: string): Promise<User> {
    return this.endpoint.registerUser(name, email);
  }
}
