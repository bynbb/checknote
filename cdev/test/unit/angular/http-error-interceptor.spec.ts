import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { httpErrorInterceptor } from '@cdev/common/composition';
import { firstValueFrom } from 'rxjs';
import { describe, expect, it } from 'vitest';

describe('httpErrorInterceptor', () => {
  it('normalizes problem-details API failures into application errors', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    const response = firstValueFrom(TestBed.inject(HttpClient).get('/api/todos'));

    TestBed.inject(HttpTestingController).expectOne('/api/todos').flush(
      {
        title: 'Validation failed',
        detail: 'The request was invalid.',
        errors: [
          {
            code: 'Todos.ReservedTitle',
            description: 'The todo title 88888 is reserved.',
          },
        ],
      },
      {
        status: 400,
        statusText: 'Bad Request',
      },
    );

    await expect(response).rejects.toMatchObject({
      code: 'Todos.ReservedTitle',
      message: 'The todo title 88888 is reserved.',
      type: 'validation',
    });

    TestBed.inject(HttpTestingController).verify();
  });

  it('normalizes failed API responses even when the body is not problem details', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    const response = firstValueFrom(TestBed.inject(HttpClient).get('/api/todos'));

    TestBed.inject(HttpTestingController).expectOne('/api/todos').flush(
      'server exploded',
      {
        status: 500,
        statusText: 'Internal Server Error',
      },
    );

    await expect(response).rejects.toMatchObject({
      code: 'Http.500',
      message: 'API request failed: 500',
      type: 'unexpected',
    });

    TestBed.inject(HttpTestingController).verify();
  });
});
