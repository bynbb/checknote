import { MockEndpointLogger } from '@cdev/common/infrastructure';
import { Todo } from '@cdev/modules/todos/domain';

export class MockTodosEndpoint {
  constructor(private readonly logger: MockEndpointLogger) {}

  loadTodos(): void {
    this.logger.log('GET /api/todos');
  }

  saveTodos(todos: readonly Todo[]): void {
    this.logger.log(`PUT /api/todos (${todos.length} todos)`);
  }
}
