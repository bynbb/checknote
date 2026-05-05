import { TodoIdGenerator } from '@cdev/modules/todos/application';
import { TodoId } from '@cdev/modules/todos/domain';

export class DateNowTodoIdGenerator implements TodoIdGenerator {
  nextId(): TodoId {
    return Date.now();
  }
}
