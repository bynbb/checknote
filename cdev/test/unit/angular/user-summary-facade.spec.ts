import { TestBed } from '@angular/core/testing';
import { ErrorReporter } from '@cdev/common/application';
import { AuthClient, AuthSession } from '@cdev/common/application';
import { AUTH_CLIENT, ERROR_REPORTER } from '@cdev/common/composition';
import { GET_CURRENT_USER_HANDLER } from '@cdev/modules/users/composition/users/users.tokens';
import { User } from '@cdev/modules/users/domain';
import { UserSummaryFacade } from '@cdev/modules/users/presentation';
import { describe, expect, it, vi } from 'vitest';

class FakeAuthClient implements AuthClient {
  private listeners: Array<(state: AuthSession) => void> = [];
  readonly login = vi.fn(() => Promise.resolve());
  readonly logout = vi.fn(() => Promise.resolve());

  constructor(private state: AuthSession) {}

  initialize(): Promise<void> {
    return Promise.resolve();
  }

  getState(): AuthSession {
    return this.state;
  }

  subscribe(listener: (state: AuthSession) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((candidate) => candidate !== listener);
    };
  }

  getAccessToken(): Promise<string | null> {
    return Promise.resolve(this.state.status === 'authenticated' ? 'token-123' : null);
  }

  setState(state: AuthSession): void {
    this.state = state;
    for (const listener of this.listeners) {
      listener(state);
    }
  }
}

class FakeErrorReporter implements ErrorReporter {
  readonly report = vi.fn();
  readonly clear = vi.fn();
}

describe('UserSummaryFacade', () => {
  it('loads the current Checknote user only after Keycloak authentication exists', async () => {
    const auth = new FakeAuthClient({ status: 'anonymous' });
    const user: User = {
      id: 'd54f3510-f44f-462e-bd97-05df139f3644',
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    };
    const handler = {
      handle: vi.fn(() => Promise.resolve(user)),
    };

    TestBed.configureTestingModule({
      providers: [
        UserSummaryFacade,
        { provide: AUTH_CLIENT, useValue: auth },
        { provide: ERROR_REPORTER, useClass: FakeErrorReporter },
        { provide: GET_CURRENT_USER_HANDLER, useValue: handler },
      ],
    });

    const facade = TestBed.inject(UserSummaryFacade);

    await facade.loadCurrentUser();
    expect(facade.user()).toBeNull();
    expect(handler.handle).not.toHaveBeenCalled();

    auth.setState({ status: 'authenticated', name: 'Ada Lovelace', email: 'ada@example.test' });
    await Promise.resolve();

    expect(handler.handle).toHaveBeenCalledTimes(1);
    expect(facade.user()).toEqual(user);
  });

  it('delegates sign-in and sign-out to the auth client', async () => {
    const auth = new FakeAuthClient({ status: 'anonymous' });

    TestBed.configureTestingModule({
      providers: [
        UserSummaryFacade,
        { provide: AUTH_CLIENT, useValue: auth },
        { provide: ERROR_REPORTER, useClass: FakeErrorReporter },
        {
          provide: GET_CURRENT_USER_HANDLER,
          useValue: { handle: vi.fn() },
        },
      ],
    });

    const facade = TestBed.inject(UserSummaryFacade);

    await facade.signIn();
    await facade.signOut();

    expect(auth.login).toHaveBeenCalledTimes(1);
    expect(auth.logout).toHaveBeenCalledTimes(1);
  });

  it('does not reload the current user when unchanged authenticated state is republished', async () => {
    const auth = new FakeAuthClient({ status: 'authenticated', name: 'Ada Lovelace', email: 'ada@example.test' });
    const user: User = {
      id: 'd54f3510-f44f-462e-bd97-05df139f3644',
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    };
    const handler = {
      handle: vi.fn(() => Promise.resolve(user)),
    };

    TestBed.configureTestingModule({
      providers: [
        UserSummaryFacade,
        { provide: AUTH_CLIENT, useValue: auth },
        { provide: ERROR_REPORTER, useClass: FakeErrorReporter },
        { provide: GET_CURRENT_USER_HANDLER, useValue: handler },
      ],
    });

    TestBed.inject(UserSummaryFacade);
    await Promise.resolve();

    expect(handler.handle).toHaveBeenCalledTimes(1);

    auth.setState({ status: 'authenticated', name: 'Ada Lovelace', email: 'ada@example.test' });
    await Promise.resolve();

    expect(handler.handle).toHaveBeenCalledTimes(1);
  });
});
