import { Command } from '@cdev/common/application';

export class AddTodoCommand implements Command {
  readonly type = 'todos.add';

  constructor(readonly title: string) {}
}
