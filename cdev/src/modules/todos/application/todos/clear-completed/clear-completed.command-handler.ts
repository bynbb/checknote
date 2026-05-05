import { CommandHandler } from '@cdev/common/application';
import { TodoRepository } from '../../abstractions/todo-repository';
import { ClearCompletedCommand } from './clear-completed.command';

export class ClearCompletedCommandHandler implements CommandHandler<ClearCompletedCommand> {
  constructor(private readonly repository: TodoRepository) {}

  async handle(_command: ClearCompletedCommand): Promise<void> {
    const todos = await this.repository.getAll();
    await this.repository.saveAll(todos.filter((todo) => !todo.completed));
  }
}
