import { ensureOk } from '@cdev/common/infrastructure';
import { TodoRepository } from '@cdev/modules/todos/application';
import { Todo } from '@cdev/modules/todos/domain';

interface ApiTodo {
  readonly id: number;
  readonly title: string;
  readonly completed: boolean;
}

interface SaveTaskListRequest {
  readonly todos: readonly ApiTodo[];
}

export class HttpTodoRepository implements TodoRepository {
  private readonly route = '/api/todos';

  async getAll(): Promise<Todo[]> {
    const response = await fetch(this.route);

    await ensureOk(response, 'Could not load todos');

    const todos = (await response.json()) as unknown;
    return Array.isArray(todos) ? todos.filter(isApiTodo) : [];
  }

  async saveAll(todos: Todo[]): Promise<void> {
    const request: SaveTaskListRequest = {
      todos,
    };

    const response = await fetch(`${this.route}/task-list`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    await ensureOk(response, 'Could not save todos');
  }
}

function isApiTodo(value: unknown): value is ApiTodo {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const todo = value as Partial<ApiTodo>;
  return typeof todo.id === 'number' && typeof todo.title === 'string' && typeof todo.completed === 'boolean';
}
