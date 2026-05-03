import { UserRepository } from '../application/abstractions/user-repository';
import { User } from '../domain/user';
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
