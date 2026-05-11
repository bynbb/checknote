import { HttpClient } from '@angular/common/http';
import { ApplicationError } from '@cdev/common/domain';
import { UserRepository } from '@cdev/modules/users/application';
import { User } from '@cdev/modules/users/domain';
import { firstValueFrom } from 'rxjs';

export class AngularHttpUserRepository implements UserRepository {
  constructor(private readonly http: HttpClient) {}

  async getCurrentUser(): Promise<User> {
    const user = await firstValueFrom(this.http.get<unknown>('/api/users/current'));

    if (isApiUser(user)) {
      return user;
    }

    throw new ApplicationError({
      code: 'Users.InvalidCurrentUserResponse',
      message: 'The current user response was not valid.',
      type: 'unexpected',
      cause: user,
    });
  }

  async register(_name: string, _email: string): Promise<User> {
    throw new ApplicationError({
      code: 'Users.RegistrationHostedByKeycloak',
      message: 'Registration is handled by Keycloak.',
      type: 'problem',
    });
  }
}

function isApiUser(value: unknown): value is User {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const user = value as Partial<User>;
  return typeof user.id === 'string' &&
    typeof user.name === 'string' &&
    typeof user.email === 'string';
}
