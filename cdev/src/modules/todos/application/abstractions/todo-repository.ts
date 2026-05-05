import { Todo } from '@cdev/modules/todos/domain';

export interface TodoRepository {
  getAll(): Promise<Todo[]>;
  saveAll(todos: Todo[]): Promise<void>;
}
