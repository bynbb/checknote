import { TodoRepository } from '../application/abstractions/todo-repository';
import { Todo } from '../domain/todo';
import { MockTodosEndpoint } from './mock-todos-endpoint';

const STORAGE_KEY = 'bazel-angular-todos';

export class LocalStorageTodoRepository implements TodoRepository {
  constructor(private readonly endpoint: MockTodosEndpoint) {}

  async getAll(): Promise<Todo[]> {
    this.endpoint.loadTodos();
    const storedTodos = localStorage.getItem(STORAGE_KEY);

    if (!storedTodos) {
      return [
        {
          id: 1,
          title: 'Build the app with Bazel',
          completed: false,
        },
      ];
    }

    try {
      const parsed = JSON.parse(storedTodos);
      return Array.isArray(parsed) ? parsed.filter(isTodo) : [];
    } catch {
      return [];
    }
  }

  async saveAll(todos: Todo[]): Promise<void> {
    this.endpoint.saveTodos(todos);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(todos));
  }
}

function isTodo(value: unknown): value is Todo {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const todo = value as Partial<Todo>;
  return typeof todo.id === 'number' && typeof todo.title === 'string' && typeof todo.completed === 'boolean';
}
