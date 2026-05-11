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
});
