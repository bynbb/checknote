import { inject, Injectable, signal } from '@angular/core';
import { GET_CURRENT_USER_HANDLER } from '../composition/users.tokens';
import { User } from '../domain/user';
import { GetCurrentUserQuery } from '../application/users/get-current-user/get-current-user.query';

@Injectable()
export class UserSummaryFacade {
  private readonly getCurrentUserHandler = inject(GET_CURRENT_USER_HANDLER);

  readonly user = signal<User | null>(null);

  constructor() {
    void this.loadCurrentUser();
  }

  async loadCurrentUser(): Promise<void> {
    this.user.set(await this.getCurrentUserHandler.handle(new GetCurrentUserQuery()));
  }
}
