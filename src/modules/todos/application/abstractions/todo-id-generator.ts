import { TodoId } from '../../domain/todo-id';

export interface TodoIdGenerator {
  nextId(): TodoId;
}
