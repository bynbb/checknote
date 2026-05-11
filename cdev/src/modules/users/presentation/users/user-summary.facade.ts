import { computed, inject, Injectable, signal } from '@angular/core';
import { AuthSession } from '@cdev/common/application';
import { AUTH_CLIENT, ERROR_REPORTER } from '@cdev/common/composition';
import { GetCurrentUserQuery } from '@cdev/modules/users/application';
import { GET_CURRENT_USER_HANDLER } from '@cdev/modules/users/composition/users/users.tokens';
import { User } from '@cdev/modules/users/domain';

@Injectable()
export class UserSummaryFacade {
  private readonly getCurrentUserHandler = inject(GET_CURRENT_USER_HANDLER);
  private readonly authClient = inject(AUTH_CLIENT);
  private readonly errors = inject(ERROR_REPORTER);

  readonly auth = signal<AuthSession>(this.authClient.getState());
  readonly user = signal<User | null>(null);
  readonly loadingUser = signal(false);
  readonly displayName = computed(() => this.user()?.name || this.auth().name || 'Signed in');
  readonly displayEmail = computed(() => this.user()?.email || this.auth().email || '');

  constructor() {
    this.authClient.subscribe((state) => {
      this.auth.set(state);
      void this.loadCurrentUser();
    });

    void this.loadCurrentUser();
  }

  async loadCurrentUser(): Promise<void> {
    if (this.auth().status !== 'authenticated') {
      this.user.set(null);
      return;
    }

    this.loadingUser.set(true);

    try {
      this.user.set(await this.getCurrentUserHandler.handle(new GetCurrentUserQuery()));
    } catch (error) {
      this.errors.report(error);
    } finally {
      this.loadingUser.set(false);
    }
  }

  async signIn(): Promise<void> {
    this.errors.clear();

    try {
      await this.authClient.login();
    } catch (error) {
      this.errors.report(error);
    }
  }

  async signOut(): Promise<void> {
    this.errors.clear();

    try {
      await this.authClient.logout();
    } catch (error) {
      this.errors.report(error);
    }
  }
}
