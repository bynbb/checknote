import { Command } from '@cdev/common/application';

export class ClearCompletedCommand implements Command {
  readonly type = 'todos.clear-completed';
}
