import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthClient, AuthSession } from '@cdev/common/application';
import { AUTH_CLIENT, authTokenInterceptor } from '@cdev/common/composition';
import { firstValueFrom } from 'rxjs';
import { describe, expect, it } from 'vitest';

class FakeAuthClient implements AuthClient {
  constructor(private readonly accessToken: string | null) {}

  initialize(): Promise<void> {
    return Promise.resolve();
  }

  getState(): AuthSession {
    return { status: this.accessToken ? 'authenticated' : 'anonymous' };
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
    return Promise.resolve(this.accessToken);
  }
}

describe('authTokenInterceptor', () => {
  it('adds a bearer token to Checknote API requests when authenticated', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authTokenInterceptor])),
        provideHttpClientTesting(),
        { provide: AUTH_CLIENT, useValue: new FakeAuthClient('token-123') },
      ],
    });

    const response = firstValueFrom(TestBed.inject(HttpClient).get('/api/users/current'));
    await Promise.resolve();

    const request = TestBed.inject(HttpTestingController).expectOne('/api/users/current');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-123');
    request.flush({});
    await response;
    TestBed.inject(HttpTestingController).verify();
  });

  it('leaves non-API requests untouched', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authTokenInterceptor])),
        provideHttpClientTesting(),
        { provide: AUTH_CLIENT, useValue: new FakeAuthClient('token-123') },
      ],
    });

    TestBed.inject(HttpClient).get('/assets/config.json').subscribe();

    const request = TestBed.inject(HttpTestingController).expectOne('/assets/config.json');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
    TestBed.inject(HttpTestingController).verify();
  });
});
