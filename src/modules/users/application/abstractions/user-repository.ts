import { User } from '../../domain/user';

export interface UserRepository {
  getCurrentUser(): Promise<User>;
  register(name: string, email: string): Promise<User>;
}
