import { Command } from '../../../../../common/application/messaging/command';

export class AddTodoCommand implements Command {
  readonly type = 'todos.add';

  constructor(readonly title: string) {}
}
