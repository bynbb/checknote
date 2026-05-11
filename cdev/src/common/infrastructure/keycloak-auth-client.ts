import { AuthClient, AuthSession } from '@cdev/common/application';
import Keycloak, { KeycloakInstance, KeycloakTokenParsed } from 'keycloak-js';

interface PublicAuthConfig {
  readonly enabled: boolean;
  readonly authServerUrl: string;
  readonly realm: string;
  readonly clientId: string;
}

type FetchJson = (url: string) => Promise<unknown>;

export class KeycloakAuthClient implements AuthClient {
  private keycloak: KeycloakInstance | null = null;
  private state: AuthSession = { status: 'loading' };
  private readonly listeners: Array<(state: AuthSession) => void> = [];

  constructor(
    private readonly fetchJson: FetchJson = defaultFetchJson,
    private readonly browserLocation: Location = window.location,
  ) {}

  async initialize(): Promise<void> {
    try {
      const config = readPublicAuthConfig(await this.fetchJson('/api/auth/config'));

      if (!config.enabled) {
        this.setState({
          status: 'unavailable',
          reason: 'Authentication is not configured for this environment.',
        });
        return;
      }

      this.keycloak = new Keycloak({
        url: config.authServerUrl,
        realm: config.realm,
        clientId: config.clientId,
      });

      const authenticated = await this.keycloak.init({
        onLoad: 'check-sso',
        pkceMethod: 'S256',
        checkLoginIframe: false,
        silentCheckSsoFallback: false,
      });

      this.setState(authenticated ? toAuthenticatedSession(this.keycloak.tokenParsed) : { status: 'anonymous' });
    } catch {
      this.keycloak = null;
      this.setState({
        status: 'unavailable',
        reason: 'Authentication could not be initialized.',
      });
    }
  }

  getState(): AuthSession {
    return this.state;
  }

  subscribe(listener: (state: AuthSession) => void): () => void {
    this.listeners.push(listener);
    return () => {
      const index = this.listeners.indexOf(listener);
      if (index >= 0) {
        this.listeners.splice(index, 1);
      }
    };
  }

  async login(): Promise<void> {
    await this.keycloak?.login({ redirectUri: this.browserLocation.origin });
  }

  async logout(): Promise<void> {
    await this.keycloak?.logout({ redirectUri: this.browserLocation.origin });
  }

  async getAccessToken(): Promise<string | null> {
    if (!this.keycloak?.authenticated) {
      return null;
    }

    try {
      await this.keycloak.updateToken(30);
      this.setState(toAuthenticatedSession(this.keycloak.tokenParsed));
      return this.keycloak.token ?? null;
    } catch {
      this.setState({ status: 'anonymous' });
      return null;
    }
  }

  private setState(state: AuthSession): void {
    this.state = state;
    for (const listener of this.listeners) {
      listener(state);
    }
  }
}

async function defaultFetchJson(url: string): Promise<unknown> {
  const response = await fetch(url, {
    headers: {
      Accept: 'application/json',
    },
  });

  return response.json() as Promise<unknown>;
}

function readPublicAuthConfig(value: unknown): PublicAuthConfig {
  if (!value || typeof value !== 'object') {
    return disabledConfig();
  }

  const config = value as Partial<PublicAuthConfig>;
  return {
    enabled: config.enabled === true,
    authServerUrl: readText(config.authServerUrl),
    realm: readText(config.realm),
    clientId: readText(config.clientId),
  };
}

function disabledConfig(): PublicAuthConfig {
  return {
    enabled: false,
    authServerUrl: '',
    realm: '',
    clientId: '',
  };
}

function readText(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

function toAuthenticatedSession(token: KeycloakTokenParsed | undefined): AuthSession {
  return {
    status: 'authenticated',
    name: readText(token?.['name']) || readText(token?.['preferred_username']),
    email: readText(token?.['email']),
  };
}
