import { AuthClient, AuthSession } from '@cdev/common/application';
import Keycloak, { KeycloakTokenParsed } from 'keycloak-js';

interface PublicAuthConfig {
  readonly enabled: boolean;
  readonly authServerUrl: string;
  readonly realm: string;
  readonly clientId: string;
}

type FetchJson = (url: string) => Promise<unknown>;
type CreateKeycloak = (config: PublicAuthConfig) => ChecknoteKeycloakClient;

interface ChecknoteKeycloakClient {
  readonly authenticated?: boolean;
  readonly token?: string;
  readonly tokenParsed?: KeycloakTokenParsed;

  init(options: { pkceMethod: 'S256'; checkLoginIframe: false }): Promise<boolean>;

  login(options: { redirectUri: string }): Promise<void>;

  logout(options: { redirectUri: string }): Promise<void>;

  updateToken(minValidity: number): Promise<boolean>;
}

export class KeycloakAuthClient implements AuthClient {
  private keycloak: ChecknoteKeycloakClient | null = null;
  private state: AuthSession = { status: 'loading' };
  private readonly listeners: Array<(state: AuthSession) => void> = [];

  constructor(
    private readonly fetchJson: FetchJson = defaultFetchJson,
    private readonly browserLocation: Location = window.location,
    private readonly createKeycloak: CreateKeycloak = defaultCreateKeycloak,
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

      this.keycloak = this.createKeycloak(config);

      const authenticated = await this.keycloak.init({
        pkceMethod: 'S256',
        checkLoginIframe: false,
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
    await this.keycloak?.login({ redirectUri: getAppRootUrl(this.browserLocation) });
  }

  async logout(): Promise<void> {
    await this.keycloak?.logout({ redirectUri: getAppRootUrl(this.browserLocation) });
  }

  async getAccessToken(): Promise<string | null> {
    if (!this.keycloak?.authenticated) {
      return null;
    }

    try {
      await this.keycloak.updateToken(30);
      const authenticatedState = toAuthenticatedSession(this.keycloak.tokenParsed);

      if (!sameAuthSession(this.state, authenticatedState)) {
        this.setState(authenticatedState);
      }

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

function defaultCreateKeycloak(config: PublicAuthConfig): ChecknoteKeycloakClient {
  return new Keycloak({
    url: config.authServerUrl,
    realm: config.realm,
    clientId: config.clientId,
  });
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

function getAppRootUrl(location: Location): string {
  return `${location.origin}/`;
}

function toAuthenticatedSession(token: KeycloakTokenParsed | undefined): AuthSession {
  return {
    status: 'authenticated',
    name: readText(token?.['name']) || readText(token?.['preferred_username']),
    email: readText(token?.['email']),
  };
}

function sameAuthSession(left: AuthSession, right: AuthSession): boolean {
  if (left.status !== right.status) {
    return false;
  }

  if (left.status === 'authenticated' && right.status === 'authenticated') {
    return left.name === right.name && left.email === right.email;
  }

  if (left.status === 'unavailable' && right.status === 'unavailable') {
    return left.reason === right.reason;
  }

  return true;
}
