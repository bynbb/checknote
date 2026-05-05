import { User } from '@cdev/modules/users/domain';

export interface UserRepository {
  getCurrentUser(): Promise<User>;
  register(name: string, email: string): Promise<User>;
}
