export type AuthStatus = 'loading' | 'anonymous' | 'authenticated' | 'unavailable';

export interface AuthSession {
  readonly status: AuthStatus;
  readonly name?: string;
  readonly email?: string;
  readonly reason?: string;
}

export interface AuthClient {
  initialize(): Promise<void>;
  getState(): AuthSession;
  subscribe(listener: (state: AuthSession) => void): () => void;
  login(): Promise<void>;
  logout(): Promise<void>;
  getAccessToken(): Promise<string | null>;
}
