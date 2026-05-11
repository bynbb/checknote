import { HttpClient } from '@angular/common/http';
import { TodoRepository } from '@cdev/modules/todos/application';
import { Todo } from '@cdev/modules/todos/domain';
import { firstValueFrom } from 'rxjs';

interface ApiTodo {
  readonly id: number;
  readonly title: string;
  readonly completed: boolean;
}

interface SaveTaskListRequest {
  readonly todos: readonly ApiTodo[];
}

export class AngularHttpTodoRepository implements TodoRepository {
  private readonly route = '/api/todos';

  constructor(private readonly http: HttpClient) {}

  async getAll(): Promise<Todo[]> {
    const todos = await firstValueFrom(this.http.get<unknown>(this.route));
    return Array.isArray(todos) ? todos.filter(isApiTodo) : [];
  }

  async saveAll(todos: Todo[]): Promise<void> {
    const request: SaveTaskListRequest = {
      todos,
    };

    await firstValueFrom(this.http.put<void>(`${this.route}/task-list`, request));
  }
}

function isApiTodo(value: unknown): value is ApiTodo {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const todo = value as Partial<ApiTodo>;
  return typeof todo.id === 'number' && typeof todo.title === 'string' && typeof todo.completed === 'boolean';
}
