import { TodoIdGenerator } from '../application/abstractions/todo-id-generator';
import { TodoId } from '../domain/todo-id';

export class DateNowTodoIdGenerator implements TodoIdGenerator {
  nextId(): TodoId {
    return Date.now();
  }
}
