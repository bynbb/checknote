import { KeycloakAuthClient } from '@cdev/common/infrastructure';
import { afterEach, describe, expect, it, vi } from 'vitest';

describe('KeycloakAuthClient', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('marks auth unavailable when public config is disabled', async () => {
    const observed: string[] = [];
    const client = new KeycloakAuthClient(
      () => Promise.resolve({
        enabled: false,
        authServerUrl: '',
        realm: 'checknote',
        clientId: 'checknote-angular',
      }),
      { origin: 'http://127.0.0.1:4200' } as Location,
    );

    client.subscribe((state) => observed.push(state.status));

    await client.initialize();

    expect(client.getState()).toMatchObject({
      status: 'unavailable',
    });
    expect(await client.getAccessToken()).toBeNull();
    expect(observed).toEqual(['unavailable']);
  });

  it('does not redirect to Keycloak while initializing an anonymous browser session', async () => {
    const observed: string[] = [];
    const keycloak = new FakeKeycloak(false);
    const client = new KeycloakAuthClient(
      () => Promise.resolve({
        enabled: true,
        authServerUrl: 'https://auth.checknote.io',
        realm: 'checknote',
        clientId: 'checknote-angular',
      }),
      { origin: 'https://www.checknote.io' } as Location,
      () => keycloak,
    );

    client.subscribe((state) => observed.push(state.status));

    await client.initialize();

    expect(client.getState()).toMatchObject({
      status: 'anonymous',
    });
    expect(keycloak.initOptions).toEqual({
      pkceMethod: 'S256',
      checkLoginIframe: false,
    });
    expect(keycloak.loginCalls).toEqual([]);
    expect(observed).toEqual(['anonymous']);
  });

  it('uses the normalized current app root when the user explicitly signs in', async () => {
    const keycloak = new FakeKeycloak(false);
    const client = new KeycloakAuthClient(
      () => Promise.resolve({
        enabled: true,
        authServerUrl: 'https://auth.checknote.io',
        realm: 'checknote',
        clientId: 'checknote-angular',
      }),
      { origin: 'https://www.checknote.io' } as Location,
      () => keycloak,
    );

    await client.initialize();
    await client.login();

    expect(keycloak.loginCalls).toEqual([{ redirectUri: 'https://www.checknote.io/' }]);
  });

  it('does not republish unchanged authenticated state while reading an access token', async () => {
    const observed: string[] = [];
    const keycloak = new FakeKeycloak(true, {
      token: 'token-123',
      tokenParsed: {
        name: 'Ada Lovelace',
        email: 'ada@example.test',
      },
    });
    const client = new KeycloakAuthClient(
      () => Promise.resolve({
        enabled: true,
        authServerUrl: 'https://auth.checknote.io',
        realm: 'checknote',
        clientId: 'checknote-angular',
      }),
      { origin: 'https://www.checknote.io' } as Location,
      () => keycloak,
    );

    client.subscribe((state) => observed.push(state.status));

    await client.initialize();
    observed.length = 0;

    expect(await client.getAccessToken()).toBe('token-123');
    expect(keycloak.updateTokenCalls).toBe(1);
    expect(observed).toEqual([]);
  });

  it('does not leave app startup waiting when Keycloak initialization never settles', async () => {
    vi.useFakeTimers();
    const observed: string[] = [];
    const keycloak = new FakeKeycloak(new Promise<boolean>(() => undefined));
    const client = new KeycloakAuthClient(
      () => Promise.resolve({
        enabled: true,
        authServerUrl: 'https://auth.checknote.io',
        realm: 'checknote',
        clientId: 'checknote-angular',
      }),
      { origin: 'https://www.checknote.io' } as Location,
      () => keycloak,
    );

    client.subscribe((state) => observed.push(state.status));

    let initialized = false;
    void client.initialize().then(() => {
      initialized = true;
    });

    await vi.advanceTimersByTimeAsync(10000);
    await Promise.resolve();

    expect(initialized).toBe(true);
    expect(client.getState()).toMatchObject({
      status: 'unavailable',
      reason: 'Authentication could not be initialized.',
    });
    expect(observed).toEqual(['unavailable']);
  });
});

class FakeKeycloak {
  readonly loginCalls: Array<{ redirectUri?: string }> = [];
  initOptions: unknown = null;
  authenticated = false;
  token: string | undefined;
  tokenParsed: Record<string, unknown> | undefined;
  updateTokenCalls = 0;

  constructor(
    private readonly initResult: boolean | Promise<boolean>,
    seed: { token?: string; tokenParsed?: Record<string, unknown> } = {},
  ) {
    this.token = seed.token;
    this.tokenParsed = seed.tokenParsed;
  }

  init(options: unknown): Promise<boolean> {
    this.initOptions = options;
    if (typeof this.initResult === 'boolean') {
      this.authenticated = this.initResult;
    }

    return Promise.resolve(this.initResult);
  }

  login(options: { redirectUri?: string }): Promise<void> {
    this.loginCalls.push(options);
    return Promise.resolve();
  }

  logout(): Promise<void> {
    return Promise.resolve();
  }

  updateToken(): Promise<boolean> {
    this.updateTokenCalls += 1;
    return Promise.resolve(true);
  }
}
