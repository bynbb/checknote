import { Command } from '@cdev/common/application';
import { TodoId } from '@cdev/modules/todos/domain';

export class DeleteTodoCommand implements Command {
  readonly type = 'todos.delete';

  constructor(readonly todoId: TodoId) {}
}
