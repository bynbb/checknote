import { Query } from '../../../../../common/application/messaging/query';
import { Todo } from '../../../domain/todo';

export class GetTodosQuery implements Query<Todo[]> {
  readonly type = 'todos.get';
}
