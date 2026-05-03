import { Command } from '../../../../../common/application/messaging/command';
import { TodoId } from '../../../domain/todo-id';

export class ToggleTodoCommand implements Command {
  readonly type = 'todos.toggle';

  constructor(readonly todoId: TodoId) {}
}
