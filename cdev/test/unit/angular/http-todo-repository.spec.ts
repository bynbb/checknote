import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { httpErrorInterceptor } from '@cdev/common/composition';
import { AngularHttpTodoRepository } from '@cdev/modules/todos/composition/todos/angular-http-todo-repository';
import { describe, expect, it } from 'vitest';

describe('AngularHttpTodoRepository', () => {
  it('loads todos through Angular HttpClient', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AngularHttpTodoRepository,
          useFactory: (http: HttpClient) => new AngularHttpTodoRepository(http),
          deps: [HttpClient],
        },
      ],
    });

    const todos = TestBed.inject(AngularHttpTodoRepository).getAll();

    TestBed.inject(HttpTestingController).expectOne('/api/todos').flush([
      {
        id: 1,
        title: 'One',
        completed: false,
      },
      {
        id: 'not-a-number',
        title: 'Ignored',
        completed: false,
      },
    ]);

    await expect(todos).resolves.toEqual([
      {
        id: 1,
        title: 'One',
        completed: false,
      },
    ]);

    TestBed.inject(HttpTestingController).verify();
  });

  it('saves todos through Angular HttpClient', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AngularHttpTodoRepository,
          useFactory: (http: HttpClient) => new AngularHttpTodoRepository(http),
          deps: [HttpClient],
        },
      ],
    });

    const save = TestBed.inject(AngularHttpTodoRepository).saveAll([
      {
        id: 7,
        title: 'Persist me',
        completed: true,
      },
    ]);

    const request = TestBed.inject(HttpTestingController).expectOne('/api/todos/task-list');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      todos: [
        {
          id: 7,
          title: 'Persist me',
          completed: true,
        },
      ],
    });
    request.flush(null);

    await expect(save).resolves.toBeUndefined();
    TestBed.inject(HttpTestingController).verify();
  });

  it('receives normalized application errors from the HTTP interceptor', async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AngularHttpTodoRepository,
          useFactory: (http: HttpClient) => new AngularHttpTodoRepository(http),
          deps: [HttpClient],
        },
      ],
    });

    const save = TestBed.inject(AngularHttpTodoRepository).saveAll([
      {
        id: 88888,
        title: '88888',
        completed: false,
      },
    ]);

    TestBed.inject(HttpTestingController).expectOne('/api/todos/task-list').flush(
      {
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

    await expect(save).rejects.toMatchObject({
      code: 'Todos.ReservedTitle',
      message: 'The todo title 88888 is reserved.',
      type: 'validation',
    });

    TestBed.inject(HttpTestingController).verify();
  });
});
