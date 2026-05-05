import { CommandHandler } from '@cdev/common/application';
import { TodoRepository } from '../../abstractions/todo-repository';
import { DeleteTodoCommand } from './delete-todo.command';

export class DeleteTodoCommandHandler implements CommandHandler<DeleteTodoCommand> {
  constructor(private readonly repository: TodoRepository) {}

  async handle(command: DeleteTodoCommand): Promise<void> {
    const todos = await this.repository.getAll();
    await this.repository.saveAll(todos.filter((todo) => todo.id !== command.todoId));
  }
}
