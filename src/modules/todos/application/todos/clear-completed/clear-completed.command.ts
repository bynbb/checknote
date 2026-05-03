import { Command } from '../../../../../common/application/messaging/command';

export class ClearCompletedCommand implements Command {
  readonly type = 'todos.clear-completed';
}
