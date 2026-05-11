import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { httpErrorInterceptor } from '@cdev/common/composition';
import { AngularHttpUserRepository } from '@cdev/modules/users/composition/users/angular-http-user-repository';
import { describe, expect, it } from 'vitest';

describe('AngularHttpUserRepository', () => {
  it('loads the current user through Angular HttpClient', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AngularHttpUserRepository,
          useFactory: (http: HttpClient) => new AngularHttpUserRepository(http),
          deps: [HttpClient],
        },
      ],
    });

    const currentUser = TestBed.inject(AngularHttpUserRepository).getCurrentUser();

    TestBed.inject(HttpTestingController).expectOne('/api/users/current').flush({
      id: 'd54f3510-f44f-462e-bd97-05df139f3644',
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    });

    await expect(currentUser).resolves.toEqual({
      id: 'd54f3510-f44f-462e-bd97-05df139f3644',
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    });

    TestBed.inject(HttpTestingController).verify();
  });

  it('rejects malformed current-user responses as application errors', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AngularHttpUserRepository,
          useFactory: (http: HttpClient) => new AngularHttpUserRepository(http),
          deps: [HttpClient],
        },
      ],
    });

    const currentUser = TestBed.inject(AngularHttpUserRepository).getCurrentUser();

    TestBed.inject(HttpTestingController).expectOne('/api/users/current').flush({
      id: 123,
      name: 'Ada Lovelace',
      email: 'ada@example.test',
    });

    await expect(currentUser).rejects.toMatchObject({
      code: 'Users.InvalidCurrentUserResponse',
      type: 'unexpected',
    });

    TestBed.inject(HttpTestingController).verify();
  });
});
