import { Query } from '@cdev/common/application';
import { Todo } from '@cdev/modules/todos/domain';

export class GetTodosQuery implements Query<Todo[]> {
  readonly type = 'todos.get';
}
