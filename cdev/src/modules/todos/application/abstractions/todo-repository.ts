import { Todo } from '../../domain/todo';

export interface TodoRepository {
  getAll(): Promise<Todo[]>;
  saveAll(todos: Todo[]): Promise<void>;
}
