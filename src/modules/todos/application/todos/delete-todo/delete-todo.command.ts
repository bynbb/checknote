import { Command } from '../../../../../common/application/messaging/command';
import { TodoId } from '../../../domain/todo-id';

export class DeleteTodoCommand implements Command {
  readonly type = 'todos.delete';

  constructor(readonly todoId: TodoId) {}
}
