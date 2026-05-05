import { Command } from '@cdev/common/application';
import { TodoId } from '@cdev/modules/todos/domain';

export class ToggleTodoCommand implements Command {
  readonly type = 'todos.toggle';

  constructor(readonly todoId: TodoId) {}
}
