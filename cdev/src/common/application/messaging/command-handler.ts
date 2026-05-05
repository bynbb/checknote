import { Command } from './command';

export interface CommandHandler<TCommand extends Command> {
  handle(command: TCommand): Promise<void>;
}
