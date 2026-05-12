import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { AuthClient, AuthSession } from '@cdev/common/application';
import { AUTH_CLIENT } from '@cdev/common/composition';
import { authenticatedRouteGuard } from '@cdev/common/composition/auth/auth-route.guard';
import { beforeEach, describe, expect, it } from 'vitest';

class StubAuthClient implements AuthClient {
  session: AuthSession = { status: 'anonymous' };

  initialize(): Promise<void> {
    return Promise.resolve();
  }

  getState(): AuthSession {
    return this.session;
  }

  subscribe(_listener: (state: AuthSession) => void): () => void {
    return () => undefined;
  }

  login(): Promise<void> {
    return Promise.resolve();
  }

  logout(): Promise<void> {
    return Promise.resolve();
  }

  getAccessToken(): Promise<string | null> {
    return Promise.resolve(null);
  }
}

describe('authenticatedRouteGuard', () => {
  let auth: StubAuthClient;

  beforeEach(() => {
    auth = new StubAuthClient();

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AUTH_CLIENT, useValue: auth },
      ],
    });
  });

  it('allows authenticated users to activate product routes', () => {
    auth.session = { status: 'authenticated', name: 'Ada Lovelace' };

    const result = runGuard();

    expect(result).toBe(true);
  });

  it('redirects anonymous users to the public sign-in route', () => {
    auth.session = { status: 'anonymous' };

    const result = runGuard();

    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/sign-in');
  });

  it('redirects unavailable auth state to the public sign-in route', () => {
    auth.session = { status: 'unavailable', reason: 'missing config' };

    const result = runGuard();

    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/sign-in');
  });
});

function runGuard() {
  return TestBed.runInInjectionContext(() =>
    authenticatedRouteGuard(
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot));
}
