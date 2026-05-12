import { KeycloakAuthClient } from '@cdev/common/infrastructure';
import { describe, expect, it } from 'vitest';

describe('KeycloakAuthClient', () => {
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

  it('uses the current app origin when the user explicitly signs in', async () => {
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

    expect(keycloak.loginCalls).toEqual([{ redirectUri: 'https://www.checknote.io' }]);
  });
});

class FakeKeycloak {
  readonly loginCalls: Array<{ redirectUri?: string }> = [];
  initOptions: unknown = null;
  authenticated = false;
  token: string | undefined;
  tokenParsed: Record<string, unknown> | undefined;

  constructor(private readonly initResult: boolean) {}

  init(options: unknown): Promise<boolean> {
    this.initOptions = options;
    this.authenticated = this.initResult;
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
    return Promise.resolve(true);
  }
}
