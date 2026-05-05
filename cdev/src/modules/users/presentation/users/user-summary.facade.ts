import { inject, Injectable, signal } from '@angular/core';
import { GetCurrentUserQuery } from '@cdev/modules/users/application';
import { GET_CURRENT_USER_HANDLER } from '@cdev/modules/users/composition/users/users.tokens';
import { User } from '@cdev/modules/users/domain';

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
