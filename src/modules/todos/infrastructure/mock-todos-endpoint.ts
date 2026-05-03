import { MockEndpointLogger } from '../../../common/infrastructure/mock-endpoint-logger';
import { Todo } from '../domain/todo';

export class MockTodosEndpoint {
  constructor(private readonly logger: MockEndpointLogger) {}

  loadTodos(): void {
    this.logger.log('GET /api/todos');
  }

  saveTodos(todos: readonly Todo[]): void {
    this.logger.log(`PUT /api/todos (${todos.length} todos)`);
  }
}
