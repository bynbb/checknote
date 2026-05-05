import { CommandHandler } from '../../../../../common/application/messaging/command-handler';
import { TodoRepository } from '../../abstractions/todo-repository';
import { ToggleTodoCommand } from './toggle-todo.command';

export class ToggleTodoCommandHandler implements CommandHandler<ToggleTodoCommand> {
  constructor(private readonly repository: TodoRepository) {}

  async handle(command: ToggleTodoCommand): Promise<void> {
    const todos = await this.repository.getAll();
    await this.repository.saveAll(
      todos.map((todo) =>
        todo.id === command.todoId
          ? {
              ...todo,
              completed: !todo.completed,
            }
          : todo,
      ),
    );
  }
}
