import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { AuthClient, AuthSession } from '@cdev/common/application';
import { AUTH_CLIENT } from '@cdev/common/composition';
import { authenticatedRouteGuard, signInRouteGuard } from '@cdev/common/composition/auth/auth-route.guard';
import { beforeEach, describe, expect, it, vi } from 'vitest';

class StubAuthClient implements AuthClient {
  session: AuthSession = { status: 'anonymous' };
  readonly login = vi.fn(() => Promise.resolve());

  initialize(): Promise<void> {
    return Promise.resolve();
  }

  getState(): AuthSession {
    return this.session;
  }

  subscribe(_listener: (state: AuthSession) => void): () => void {
    return () => undefined;
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

    const result = runAuthenticatedRouteGuard();

    expect(result).toBe(true);
  });

  it('starts Keycloak login directly for anonymous users on product routes', () => {
    auth.session = { status: 'anonymous' };

    const result = runAuthenticatedRouteGuard();

    expect(result).toBe(false);
    expect(auth.login).toHaveBeenCalledTimes(1);
  });

  it('redirects unavailable auth state to the public sign-in route', () => {
    auth.session = { status: 'unavailable', reason: 'missing config' };

    const result = runAuthenticatedRouteGuard();

    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/sign-in');
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('starts Keycloak login directly when the public sign-in route is requested anonymously', () => {
    auth.session = { status: 'anonymous' };

    const result = runSignInRouteGuard();

    expect(result).toBe(false);
    expect(auth.login).toHaveBeenCalledTimes(1);
  });

  it('renders the public sign-in route when auth is unavailable', () => {
    auth.session = { status: 'unavailable', reason: 'missing config' };

    const result = runSignInRouteGuard();

    expect(result).toBe(true);
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('redirects authenticated users away from the public sign-in route', () => {
    auth.session = { status: 'authenticated', name: 'Ada Lovelace' };

    const result = runSignInRouteGuard();

    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/');
    expect(auth.login).not.toHaveBeenCalled();
  });
});

function runAuthenticatedRouteGuard() {
  return TestBed.runInInjectionContext(() =>
    authenticatedRouteGuard(
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot));
}

function runSignInRouteGuard() {
  return TestBed.runInInjectionContext(() =>
    signInRouteGuard(
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot));
}
