import { TodoId } from '@cdev/modules/todos/domain';

export interface TodoIdGenerator {
  nextId(): TodoId;
}
