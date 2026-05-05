import { TodoId } from './todo-id';

export interface Todo {
  readonly id: TodoId;
  readonly title: string;
  readonly completed: boolean;
}
